﻿//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Timers;
//using System.Threading.Tasks;
//using System.Windows;
//using System.Windows.Input;
//using Autodesk.DesignScript.Geometry;
//using Autodesk.DesignScript.Runtime;
//using Dynamo.Wpf.ViewModels.Watch3D;
//using DynaShape.Goals;
//using DynaShape.GeometryBinders;
//using Microsoft.Win32;
//using Point = Autodesk.DesignScript.Geometry.Point;
//using Vector = Autodesk.DesignScript.Geometry.Vector;

//namespace DynaShape
//{
//    [IsVisibleInDynamoLibrary(false)]
//    public class Solver : IDisposable
//    {
//        public bool EnableMouseInteraction = true;
//        public bool EnableMomentum = true;
//        public bool EnableFastDisplay = true;

//        public List<Node> Nodes = new List<Node>();
//        public List<Goal> Goals = new List<Goal>();
//        public List<GeometryBinder> GeometryBinders = new List<GeometryBinder>();

//        internal int HandleNodeIndex = -1;
//        internal int NearestNodeIndex = -1;

//        internal DynaShapeDisplay Display;

//        internal Timer timer;


//        public Solver()
//        {
//            if (DynaShapeViewExtension.ViewModel == null) throw new Exception("Oh no, DynaShape could not get access to the Helix ViewModel. Sad!");

//            DynaShapeViewExtension.ViewModel.ViewMouseDown += ViewportMouseDownHandler;
//            DynaShapeViewExtension.ViewModel.ViewMouseUp += ViewportMouseUpHandler;
//            DynaShapeViewExtension.ViewModel.ViewMouseMove += ViewportMouseMoveHandler;
//            DynaShapeViewExtension.ViewModel.ViewCameraChanged += ViewportCameraChangedHandler;
//            DynaShapeViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged += ViewportCanNavigateBackgroundPropertyChangedHandler;

//            Display = new DynaShapeDisplay(this);

//            timer = new Timer(1) { AutoReset = false };
//            timer.Elapsed += timerElapsedEventHandler;
//        }


//        public void AddGoals(IEnumerable<Goal> goals, double nodeMergeThreshold = 0.001)
//        {
//            foreach (Goal goal in goals)
//                AddGoal(goal, nodeMergeThreshold);
//        }


//        public void AddGeometryBinders(IEnumerable<GeometryBinder> geometryBinders, double nodeMergeThreshold = 0.001)
//        {
//            foreach (GeometryBinder geometryBinder in geometryBinders)
//                AddGeometryBinder(geometryBinder, nodeMergeThreshold);
//        }


//        public void AddGoal(Goal goal, double nodeMergeThreshold = 0.001)
//        {
//            Goals.Add(goal);

//            if (goal.StartingPositions == null && goal.NodeIndices != null) return;

//            goal.NodeIndices = new int[goal.NodeCount];

//            for (int i = 0; i < goal.NodeCount; i++)
//            {
//                bool nodeAlreadyExist = false;

//                for (int j = 0; j < Nodes.Count; j++)
//                    if ((goal.StartingPositions[i] - Nodes[j].Position).LengthSquared <
//                        nodeMergeThreshold * nodeMergeThreshold)
//                    {
//                        goal.NodeIndices[i] = j;
//                        nodeAlreadyExist = true;
//                        break;
//                    }

//                if (!nodeAlreadyExist)
//                {
//                    Nodes.Add(new Node(goal.StartingPositions[i]));
//                    goal.NodeIndices[i] = Nodes.Count - 1;
//                }
//            }
//        }


//        public void AddGeometryBinder(GeometryBinder geometryBinder, double nodeMergeThreshold = 0.001)
//        {
//            GeometryBinders.Add(geometryBinder);

//            if (geometryBinder.StartingPositions == null && geometryBinder.NodeIndices != null) return;

//            geometryBinder.NodeIndices = new int[geometryBinder.NodeCount];

//            for (int i = 0; i < geometryBinder.NodeCount; i++)
//            {
//                bool nodeAlreadyExist = false;

//                for (int j = 0; j < Nodes.Count; j++)
//                    if ((geometryBinder.StartingPositions[i] - Nodes[j].Position).LengthSquared <
//                        nodeMergeThreshold * nodeMergeThreshold)
//                    {
//                        geometryBinder.NodeIndices[i] = j;
//                        nodeAlreadyExist = true;
//                        break;
//                    }

//                if (!nodeAlreadyExist)
//                {
//                    Nodes.Add(new Node(geometryBinder.StartingPositions[i]));
//                    geometryBinder.NodeIndices[i] = Nodes.Count - 1;
//                }
//            }
//        }


//        public List<Triple> GetNodePositions()
//        {
//            List<Triple> nodePositions = new List<Triple>(Nodes.Count);
//            for (int i = 0; i < Nodes.Count; i++)
//                nodePositions.Add(Nodes[i].Position);
//            return nodePositions;
//        }


//        public List<Point> GetNodePositionsAsPoints()
//        {
//            List<Point> nodePositions = new List<Point>(Nodes.Count);
//            for (int i = 0; i < Nodes.Count; i++)
//                nodePositions.Add(Nodes[i].Position.ToPoint());
//            return nodePositions;
//        }


//        public List<List<Triple>> GetStructuredNodePositions()
//        {
//            List<List<Triple>> nodePositions = new List<List<Triple>>(Goals.Count);
//            for (int i = 0; i < Goals.Count; i++)
//            {
//                List<Triple> goalNodePositions = new List<Triple>(Goals[i].NodeCount);
//                for (int j = 0; j < Goals[i].NodeCount; j++)
//                    goalNodePositions.Add(Nodes[Goals[i].NodeIndices[j]].Position);
//                nodePositions.Add(goalNodePositions);
//            }
//            return nodePositions;
//        }


//        public List<List<Point>> GetStructuredNodePositionsAsPoints()
//        {
//            List<List<Point>> nodePositions = new List<List<Point>>(Goals.Count);
//            for (int i = 0; i < Goals.Count; i++)
//            {
//                List<Point> goalNodePositions = new List<Point>(Goals[i].NodeCount);
//                for (int j = 0; j < Goals[i].NodeCount; j++)
//                    goalNodePositions.Add(Nodes[Goals[i].NodeIndices[j]].Position.ToPoint());
//                nodePositions.Add(goalNodePositions);
//            }
//            return nodePositions;
//        }


//        public List<Triple> GetNodeVelocities()
//        {
//            List<Triple> nodeVelocities = new List<Triple>(Nodes.Count);
//            for (int i = 0; i < Nodes.Count; i++)
//                nodeVelocities.Add(Nodes[i].Velocity);
//            return nodeVelocities;
//        }


//        public List<Vector> GetNodeVelocitiesAsVectors()
//        {
//            List<Vector> nodeVelocities = new List<Vector>(Nodes.Count);
//            for (int i = 0; i < Nodes.Count; i++)
//                nodeVelocities.Add(Nodes[i].Velocity.ToVector());
//            return nodeVelocities;
//        }


//        public List<List<DesignScriptEntity>> GetGeometries()
//        {
//            List<List<DesignScriptEntity>> geometries = new List<List<DesignScriptEntity>>(GeometryBinders.Count);
//            for (int i = 0; i < GeometryBinders.Count; i++)
//                geometries.Add(GeometryBinders[i].CreateGeometryObjects(Nodes));
//            return geometries;
//        }


//        public List<List<object>> GetGoalOutputs()
//        {
//            List<List<object>> goalOutputs = new List<List<object>>(Goals.Count);
//            for (int i = 0; i < Goals.Count; i++)
//                goalOutputs.Add(Goals[i].GetOutput(Nodes));
//            return goalOutputs;
//        }


//        public void Clear()
//        {
//            Nodes.Clear();
//            Goals.Clear();
//            GeometryBinders.Clear();
//        }


//        public void Reset()
//        {
//            foreach (Node node in Nodes) node.Reset();
//        }


//        public void Step()
//        {
//            //=================================================================================
//            // Apply momentum
//            //=================================================================================

//            if (EnableMomentum)
//                foreach (Node node in Nodes)
//                    node.Position += node.Velocity;


//            //=================================================================================
//            // Process each goal indepently, in parallel
//            //=================================================================================

//            Parallel.ForEach(Goals, goal => goal.Compute(Nodes));


//            //=================================================================================
//            // Compute the total move vector that acts on each node
//            //=================================================================================

//            Triple[] nodeMoveSums = new Triple[Nodes.Count];
//            float[] nodeWeightSums = new float[Nodes.Count];

//            for (int j = 0; j < Goals.Count; j++)
//            {
//                Goal goal = Goals[j];
//                for (int i = 0; i < goal.NodeCount; i++)
//                {
//                    nodeMoveSums[goal.NodeIndices[i]] += goal.Moves[i] * goal.Weight;
//                    nodeWeightSums[goal.NodeIndices[i]] += goal.Weight;
//                }
//            }

//            //=================================================================================
//            // Move the manipulated node toward the mouse ray
//            //=================================================================================

//            if (HandleNodeIndex != -1)
//            {
//                float mouseInteractionWeight = 30f;
//                nodeWeightSums[HandleNodeIndex] += mouseInteractionWeight;

//                Triple v = Nodes[HandleNodeIndex].Position - DynaShapeViewExtension.MouseRayOrigin;
//                Triple mouseRayPull = v.Dot(DynaShapeViewExtension.MouseRayDirection) * DynaShapeViewExtension.MouseRayDirection - v;
//                nodeMoveSums[HandleNodeIndex] += mouseInteractionWeight * mouseRayPull;
//            }


//            //=============================================================================================
//            // Move the nodes to their new positions
//            //=============================================================================================

//            for (int i = 0; i < Nodes.Count; i++)
//            {
//                Triple move = nodeMoveSums[i] / nodeWeightSums[i];
//                Nodes[i].Position += move;
//                if (EnableMomentum) Nodes[i].Velocity += move;
//                if (Nodes[i].Velocity.Dot(move) < 0.0) Nodes[i].Velocity *= 0.9f;
//            }
//        }


//        public void Step(int iterationCount)
//        {
//            for (int i = 0; i < iterationCount; i++) Step();
//        }


//        public void Step(double miliseconds)
//        {
//            Stopwatch stopwatch = Stopwatch.StartNew();
//            while (stopwatch.Elapsed.TotalMilliseconds < miliseconds)
//                Step();
//        }


//        internal void ActivateBackgroundExecution(bool activate)
//        {
//            timer.Enabled = activate;
//        }

//        private void ViewportCameraChangedHandler(object sender, RoutedEventArgs args)
//        {
//            NearestNodeIndex = -1;
//        }


//        private void ViewportMouseDownHandler(object sender, MouseButtonEventArgs args)
//        {
//            if (args.LeftButton == MouseButtonState.Pressed && EnableMouseInteraction)
//                HandleNodeIndex = FindNearestNodeIndex();
//        }


//        private void ViewportMouseUpHandler(object sender, MouseButtonEventArgs args)
//        {
//            HandleNodeIndex = -1;
//            NearestNodeIndex = -1;
//        }


//        private void ViewportMouseMoveHandler(object sender, MouseEventArgs args)
//        {
//            if (!EnableMouseInteraction) return;
//            if (args.LeftButton == MouseButtonState.Released) HandleNodeIndex = -1;
//            NearestNodeIndex = FindNearestNodeIndex();
//        }


//        internal int FindNearestNodeIndex(float range = 0.03f)
//        {
//            CameraData cameraData = DynaShapeViewExtension.CameraData;

//            Triple camZ = new Triple(
//                cameraData.LookDirection.X,
//                -cameraData.LookDirection.Z,
//                cameraData.LookDirection.Y).Normalise();

//            Triple camY = new Triple(
//                cameraData.UpDirection.X,
//                -cameraData.UpDirection.Z,
//                cameraData.UpDirection.Y).Normalise();

//            Triple camX = camY.Cross(camZ).Normalise();

//            Triple mousePosition2D = new Triple(
//                DynaShapeViewExtension.MouseRayDirection.Dot(camX),
//                DynaShapeViewExtension.MouseRayDirection.Dot(camY),
//                DynaShapeViewExtension.MouseRayDirection.Dot(camZ));

//            mousePosition2D /= mousePosition2D.Z;

//            int nearestNodeIndex = -1;

//            float minDistSquared = range * range;

//            for (int i = 0; i < Nodes.Count; i++)
//            {
//                Triple v = Nodes[i].Position - DynaShapeViewExtension.MouseRayOrigin;
//                v = new Triple(v.Dot(camX), v.Dot(camY), v.Dot(camZ));
//                Triple nodePosition2D = v / v.Z;

//                float distSquared = (mousePosition2D - nodePosition2D).LengthSquared;

//                if (distSquared < minDistSquared)
//                {
//                    minDistSquared = distSquared;
//                    nearestNodeIndex = i;
//                }
//            }

//            return nearestNodeIndex;
//        }


//        private void ViewportCanNavigateBackgroundPropertyChangedHandler(bool canNavigate)
//        {
//            HandleNodeIndex = -1;
//            NearestNodeIndex = -1;
//        }


//        public void Dispose()
//        {
//            if (DynaShapeViewExtension.ViewModel == null) throw new Exception("Oh no, DynaShape could not get access to the viewport.Sad!");

//            DynaShapeViewExtension.ViewModel.ViewMouseDown -= ViewportMouseDownHandler;
//            DynaShapeViewExtension.ViewModel.ViewMouseUp -= ViewportMouseUpHandler;
//            DynaShapeViewExtension.ViewModel.ViewMouseMove -= ViewportMouseMoveHandler;
//            DynaShapeViewExtension.ViewModel.ViewCameraChanged -= ViewportCameraChangedHandler;
//            DynaShapeViewExtension.ViewModel.CanNavigateBackgroundPropertyChanged -= ViewportCanNavigateBackgroundPropertyChangedHandler;

//            timer.Elapsed -= timerElapsedEventHandler;

//            Display.Dispose();
//        }


//        private void timerElapsedEventHandler(object sender, ElapsedEventArgs e)
//        {        
//            Step(25.0);
//            if (EnableFastDisplay) Display.RenderGeometries();
//            timer.Start();
//        }

//    }
//}
