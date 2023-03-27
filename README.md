# CSCI5606_ParaviewToABR

Open CSCI5606 folder in Unity.
Use DataLoader.pvsm to load state in Paraview to use data transfer pipeline
- Pipeline contains two EasyParaViewToABR filters, 1) volume data and 2) water surface
- Use naming schema LANL/Asteroid/[simulation identifier]_[Data Type]_[Timestep]
- Ex: LANL/Asteroid/B31_Volume_0

Key Data Naming Conventions:
[SimIdentifier]_[DataTypeIdentifier]_[ImpactTimestep]
DataTypeIdentifiers (should be self explanitory):
- Volume
- Water
- Meteor25
- Meteor50
- Meteor75
ImpactTimestep:
- This indexes from when you've identified the meteor impact is. For instance, the meteor impact of simulation C31 is in timestep 8. So that frames ImpactTimestep would be 0. Then from there on out you can determine a frames ImpactTimestep by subtracting the timestep # where the impact occurs. For example, the volume data for frame/timestep 18 of sim C31 would be saved as "C31_Volume_10", because it is 10 timesteps after the impact.


Tutorial:
1. Git clone this repository into your ABR-5609 folder
2. Open Unity Hub and open the CSCI5606_ParaviewToABR folder
3. Open the Unity Scene, "ParaviewToABR"
4. Import/copy any necessary ABR/IVLAB packages into your Unity project from prior homeworks
5. Press play. You should see the following logs in the console that it is "Listening for data on port 1900"
![image](https://user-images.githubusercontent.com/19377178/228014636-1a999e64-3b49-4508-9425-4e8246c5435d.png)

6. Navigate to https://oceans11.lanl.gov/deepwaterimpact/ and identify which simulation you are responsible for importing and load the 300x300x300 resolution data
Ian - A31
Matthew - C31
Audrey - B11
Levi - A32
Dat - C11
Tristan - B31
7. Open Paraview and follow the tutorial here: https://github.com/ivlab/ABREngine-UnityPackage/tree/master/EasyParaViewToABR~ to load the EasyParaViewToABR plugin.
8. Load the following state file into Paraview, CSCI5606_ParaviewToABR/ParaviewState/DataLoader.pvsm
9. Iteratively load .vti files for your simulation into paraview until you find the point of impact where the meteor touches the water (Note: it can be helpful to save the files with their SciVis timestep. Ex. Instead of saving as pv_insitu_300x300x300_16936.vti, I would save it as pv_insitu_300x300x300_32.vti bc it is the 32nd timestep)
9.1 Load files, by downloading a new timestep and using the change file feature in Paraview
![image](https://user-images.githubusercontent.com/19377178/228016390-3b8657a4-e350-47d4-8ec4-9c9f0e3e7880.png)
9.2 By exloring the first few timesteps and making the water and meteor surface visibile, I found that TS 8 was the first frame when the meteor had struck the water surface.
![image](https://user-images.githubusercontent.com/19377178/228025975-033efc56-1839-4392-8225-0655b8206711.png)
9.3 Once found, edit the keydata name in each EasyParaviewToABR filter ( Dataset: Asteroid, Organization: LANL, Key Data Name: [see Key Data Naming Conventions])
![image](https://user-images.githubusercontent.com/19377178/228026908-87edf615-0850-4f05-8367-86e5bd3e38f7.png)
9.4 Apply and make visible each EasyParaviewToABR filter. Verify Data was sent correctly in Unity by looking for the following console output: "Sent label "LANL/Asteroid/KeyData/[key_data_name]" ok
![image](https://user-images.githubusercontent.com/19377178/228038976-8533a7d9-4538-4326-882b-318de86c0a6c.png)
10. Complete relevant steps 9.1-9.4 for every 10 timesteps after the impact and if possible, every 10 timestpes prior (name prior timesteps -10,-20,etc.)
11. Push all derived data to github, but saving it in ParaviewState/Data/



