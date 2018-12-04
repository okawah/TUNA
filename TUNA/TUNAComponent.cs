using System;
using System.Collections.Generic;


using Grasshopper.Kernel;
using Rhino.Geometry;



// In order to load the result of this wizard, you will also need to
// add the output bin/ folder of this project to the list of loaded
// folder in Grasshopper.
// You can use the _GrasshopperDeveloperSettings Rhino command for that.

namespace TUNA
{
    public class TUNAComponent : GH_Component
    {
        /// <summary>
        /// Each implementation of GH_Component must provide a public 
        /// constructor without any arguments.
        /// Category represents the Tab in which the component will appear, 
        /// Subcategory the panel. If you use non-existing tab or panel names, 
        /// new tabs/panels will automatically be created.
        /// </summary>
        public TUNAComponent()
          : base("TUNA", "TUNA (*^_^*)",
              "Description",
              "TUNA", "Reinforcement Learning")
        {
        }
        static public Vector3d current_position = new Vector3d();
        static public Plane current_plane;
        Plane original_plane = new Plane();
        Mesh collision = new Mesh();
        Brain brain;
        Boolean reset = false;
        Boolean move = true;
        List<double> action = new List<double>();
        float reward = 0;
        string text_out;
        int fail_count = 0;
        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Action", "A", "Action", GH_ParamAccess.list);
            pManager.AddBooleanParameter("Move", "M", "Move", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Train", "T", "Train", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Reset Position", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Clear", "C", "Re-initialize Neural Network", GH_ParamAccess.item);
            pManager.AddPlaneParameter("Plane", "P", "Original Position", GH_ParamAccess.item);
            pManager.AddMeshParameter("Box", "B", "Collision Geometry", GH_ParamAccess.item);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddPlaneParameter("Action", "A", "Action", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Reset", "R", "Reset", GH_ParamAccess.item);
            pManager.AddVectorParameter("State", "S", "State", GH_ParamAccess.item);
            pManager.AddTextParameter("Text", "T", "Text", GH_ParamAccess.item);
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object can be used to retrieve data from input parameters and 
        /// to store data in output parameters.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {

            List<double> state;            
            Boolean train = true;
            Boolean clear = false;
            string text1 = "";
            string text2;
            

            if (!DA.GetDataList(0, action)) return;
            if (!DA.GetData(1, ref move)) return;
            if (!DA.GetData(2, ref train)) return;
            if (!DA.GetData(3, ref reset)) return;
            if (!DA.GetData(4, ref clear)) return;
            if (!DA.GetData(5, ref original_plane)) return;
            if (!DA.GetData(6, ref collision)) return;
            state = new List<double> { current_position.X, current_position.Y, current_position.Z};
            //initialize neural network
            if (brain == null || clear || current_plane == null)
            {
                current_plane = original_plane.Clone();
                brain = new Brain(state.Count, action.Count, 3, 5, 0.1);
                text1 += "plane initialized";
                text1 += "brain initialized";
                fail_count = 0;
            }

            double[] nums = current_plane.GetPlaneEquation();
            foreach(double d in nums)
            {
                state.Add(d);
            }    
            
            CheckBall(current_position);
            //choose best action for next step
            int nextActionIndex = brain.ChooseAction(state);
            if(move)
            {
                if (nextActionIndex < 2)
                {
                    current_plane.Rotate((Math.PI / 180) * action[nextActionIndex], new Vector3d(1, 0, 0));
                }
                else if (nextActionIndex < 4)
                {
                    current_plane.Rotate((Math.PI / 180) * action[nextActionIndex], new Vector3d(0, 1, 0));
                }
                else if (nextActionIndex < 6)
                {
                    current_plane.Rotate((Math.PI / 180) * action[nextActionIndex], new Vector3d(0, 0, 1));
                }
                else if (nextActionIndex < 8)
                {
                    current_plane.Translate(new Vector3d(1, 0, 0) * action[nextActionIndex]);
                }
                else if (nextActionIndex < 10)
                {
                    current_plane.Translate(new Vector3d(0, 1, 0) * action[nextActionIndex]);
                }
                else
                {
                    current_plane.Translate(new Vector3d(0, 0, 1) * action[nextActionIndex]);
                }

            }

            CheckBall(current_position);
            //train
            if (train)
            {
                brain.Train(state, reward);
            }

            if (reset) Reset();

            text2 = Convert.ToString(fail_count);

            string text_out = text1 + text2;
            DA.SetData(0, current_plane);
            DA.SetData(1, reset);
            DA.SetData(2, current_position);
            DA.SetData(3, text_out);
        }

        /// <summary>
        /// Provides an Icon for every component that will be visible in the User Interface.
        /// Icons need to be 24x24 pixels.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                // You can add image files to your project resources and access them like this:
                //return Resources.IconForThisComponent;
                return Properties.Resources.tuna;
            }
        }

        /// <summary>
        /// Each component must have a unique Guid to identify it. 
        /// It is vital this Guid doesn't change otherwise old ghx files 
        /// that use the old ID will partially fail during loading.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("4939175a-64a3-4fd5-89e4-de4ef008565d"); }
        }

        void Reset()
        {
            current_plane = original_plane.Clone();
            text_out = "";
        }

        void CheckBall(Vector3d position)
        {
            Point3d pos = new Point3d(position.X, position.Y, position.Z);
            Point3d p = collision.ClosestPoint(pos);
            double dist = pos.DistanceTo(p);
            if (dist < 110)
            {
                brain.dropped = false;
                reset = false;
                reward = 1;
            }
            else
            {
                brain.dropped = true;
                reset = true;
                reward = -1;
                Reset();
                fail_count += 1;
            }
        }
    }
}
