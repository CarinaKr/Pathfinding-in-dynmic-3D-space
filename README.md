This repository contains the results of the bachelor's thesis:

### Implementation and comparison of pathfinding algorithms in a dynamic 3D space" ###

developed at the University of Applied Sciences Hamburg.

In addition to a written paper the thesis includes a project, where the game environment "Lost in Space" was developed and the pathfinding algorithms A*, ARA*, Theta*, D* Lite, and AD* were implemented in an abstract test environment. For this project the game engine Unity 2018.2.10f1 was used.

The full project is included in the folder "LostInSpace", which can be opened as a Unity project after download. The Unity project contains three game scenes. The scene "LostInSpace" contains the game environment "Lost in Space", with its functionalities as described in the thesis. The scene "TestEnvironment" contains the abstract test environment with all the implemented algorithms. It was used for testing and comparison of the algorithms throughout the thesis. The implementation of the AI and the destination in "TestEnvironment" uses some classes from "Lost In Space". Therfore a third scene "Pathfinding" is provided, which is fully independent from the game. In addition, the Unity package "pathfindingAlgorithms" is provided for a demonstration of only the pathfinding algorithms, without any of the game,.



### Demonstration ###

For a demonstration of the pathfinding algorithms implemented in the scope of this thesis, open the scene "TestEnvironment" or "Pathfinding" in the Unity project or import the package "pathfindingAlgorithm" into Unity and open the scene "Pathfinding". Should an error message appear after import, similar to: "Assets/Plugins/AsyncAwaitUtil/Source/AwaitExtensions.cs(28,23): error CS1644: Feature 'asynchronous functions' cannot be used because it is not part of the C# 4.0 language specification", navigate to Edit->Project Settings->Player and change the "Scripting Runtime Version" to ".NET 4.x Equivalent" and restart Unity.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/scriptingRuntimeVersion.PNG">

In these scenes one can choose between a cell-grid and a waypoint-based search graph setup as the "Creation Mode"-variable of the "SearchGraphManager"-object. In the "SearchGraphManager" one can also choose the interval time.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/seachGraph.PNG">
</p>

The pathfinding algorithm can be selected as the "Pathfinding Method"-value in the "PathfindingManager" object.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/pathfindingAlgorithm.PNG">
</p>

Expanding the "PathfindingManager" one can select each algorithm to adjust public variables, such as the inflation factor, or decrease factor.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/A_star.PNG">
</p>
<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/ARA_star.PNG">
</p>

Using a cell-grid search graph, the size of the search graph and the size of the cells can be set in the "CellGridManager".

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/cellGridManager.PNG">
</p>

To change the maximum distance between waypoints in a waypoint-based search graph one can adjust the "Max Waypoint Distance"-variable of each obstacle.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/waypointObstacle.PNG">
</p>


To visualise the full search graph navigate to Assets->Prefabs->Pathfinding->Cell or accordingly to Assets->Prefabs->Pathfinding->Waypoint and check the "Show All Nodes" option.

<p align="center">
  <img width="230" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/cell.PNG">
 <img width="230" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/wp.PNG">
</p>

Whilst the game runs, go to the Scene View and select the "SearchGraphManager", when using cell-grid, or the "Environment", when using waypoints.

<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/visualizeCellGrid.PNG">
</p>
<p align="center">
  <img width="460" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/visualizeWP.PNG">
</p>

Note, that by visualising the search graph the framerate decreases significantly.

For visualising the progress of the pathfinding algorithm, select the according algorithm in the hierarchy and check the "Visualize" box.

<p align="center">
  <img width="300" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/A_star-visualize-true.PNG">
</p>

Selecting the "SearchGraphManager" or "Environment" accordingly, one can then see all nodes in the open list 
framed in magenta and all nodes in the closed list framed in grey. Running dynamic algorithms with a static destination, one can also see all nodes that have been changed in the last update of the search graph indicated in blue, and all nodes that have been added to the open list after the last search-graph update in yellow. Please note: Visualising the progress of the pathfinding algorithm this way, increases the runtime of the algorithm significantly. Additionally, for a better overview it is advised to uncheck "Show All Nodes" when using this option.

<p align="center">
  <img width="550" src="https://github.com/CarinaKr/Pathfinding-in-dynmic-3D-space/tree/master/readmeImages/visualizeA_star.PNG">
</p>


