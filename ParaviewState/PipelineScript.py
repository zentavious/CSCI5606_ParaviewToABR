# trace generated using paraview version 5.11.0
#import paraview
#paraview.compatibility.major = 5
#paraview.compatibility.minor = 11

#### import the simple module from the paraview
from paraview.simple import *
#### disable automatic camera reset on 'Show'
paraview.simple._DisableFirstRenderCameraReset()

# load plugin
LoadPlugin('C:\\Users\\Zenta_t2ma3ok\\EasyParaViewToABR\\EasyParaViewToABR.py', remote=False, ns=globals())

# create a new 'XML Image Data Reader'
pv_insitu_300x300x300_68vti = XMLImageDataReader(registrationName='pv_insitu_300x300x300_68.vti', FileName=['C:\\Users\\Zenta_t2ma3ok\\Documents\\Visualization\\ProjectData\\B31\\pv_insitu_300x300x300_68.vti'])
pv_insitu_300x300x300_68vti.CellArrayStatus = ['vtkGhostType']
pv_insitu_300x300x300_68vti.PointArrayStatus = ['prs', 'rho', 'tev', 'v02', 'v03', 'vtkValidPointMask', 'vtkGhostType']

# Properties modified on pv_insitu_300x300x300_68vti
pv_insitu_300x300x300_68vti.TimeArray = 'None'

UpdatePipeline(time=0.0, proxy=pv_insitu_300x300x300_68vti)

# create a new 'Resample To Image'
resampleToImage1 = ResampleToImage(registrationName='ResampleToImage1', Input=pv_insitu_300x300x300_68vti)
resampleToImage1.SamplingBounds = [-2300000.0, 2300000.0001149997, -500000.0, 2300000.000005, -1200000.0, 1199999.9999872]

UpdatePipeline(time=0.0, proxy=resampleToImage1)

# create a new 'EasyParaViewToABR'
easyParaViewToABR1 = EasyParaViewToABR(registrationName='EasyParaViewToABR1', InputDataset=resampleToImage1)

# Properties modified on easyParaViewToABR1
easyParaViewToABR1.a2Dataset = 'Asteroid'
easyParaViewToABR1.a3KeyDataName = 'B31_Volume_60'
easyParaViewToABR1.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR1)

# set active source
SetActiveSource(pv_insitu_300x300x300_68vti)

# create a new 'Contour'
contour1 = Contour(registrationName='Contour1', Input=pv_insitu_300x300x300_68vti)
contour1.ContourBy = ['POINTS', 'prs']
contour1.Isosurfaces = [273454990.671875]
contour1.PointMergeMethod = 'Uniform Binning'

# Properties modified on contour1
contour1.ContourBy = ['POINTS', 'v02']
contour1.Isosurfaces = [1.0]

UpdatePipeline(time=0.0, proxy=contour1)

# create a new 'EasyParaViewToABR'
easyParaViewToABR2 = EasyParaViewToABR(registrationName='EasyParaViewToABR2', InputDataset=contour1)

# Properties modified on easyParaViewToABR2
easyParaViewToABR2.a2Dataset = 'Asteroid'
easyParaViewToABR2.a3KeyDataName = 'B31_Water_60'
easyParaViewToABR2.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR2)

# set active source
SetActiveSource(pv_insitu_300x300x300_68vti)

# create a new 'Contour'
contour2 = Contour(registrationName='Contour2', Input=pv_insitu_300x300x300_68vti)
contour2.ContourBy = ['POINTS', 'prs']
contour2.Isosurfaces = [273454990.671875]
contour2.PointMergeMethod = 'Uniform Binning'

# Properties modified on contour2
contour2.ContourBy = ['POINTS', 'v03']
contour2.Isosurfaces = [0.75]

UpdatePipeline(time=0.0, proxy=contour2)

# create a new 'EasyParaViewToABR'
easyParaViewToABR3 = EasyParaViewToABR(registrationName='EasyParaViewToABR3', InputDataset=contour2)

# Properties modified on easyParaViewToABR3
easyParaViewToABR3.a2Dataset = 'Asteroid'
easyParaViewToABR3.a3KeyDataName = 'B31_Asteroid75_60'
easyParaViewToABR3.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR3)

# set active source
SetActiveSource(pv_insitu_300x300x300_68vti)

# create a new 'Contour'
contour3 = Contour(registrationName='Contour3', Input=pv_insitu_300x300x300_68vti)
contour3.ContourBy = ['POINTS', 'prs']
contour3.Isosurfaces = [273454990.671875]
contour3.PointMergeMethod = 'Uniform Binning'

# Properties modified on contour3
contour3.ContourBy = ['POINTS', 'v03']
contour3.Isosurfaces = [0.5]

UpdatePipeline(time=0.0, proxy=contour3)

# create a new 'EasyParaViewToABR'
easyParaViewToABR4 = EasyParaViewToABR(registrationName='EasyParaViewToABR4', InputDataset=contour3)

# Properties modified on easyParaViewToABR4
easyParaViewToABR4.a2Dataset = 'Asteroid'
easyParaViewToABR4.a3KeyDataName = 'B31_Asteroid50_60'
easyParaViewToABR4.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR4)

# set active source
SetActiveSource(pv_insitu_300x300x300_68vti)

# create a new 'Contour'
contour4 = Contour(registrationName='Contour4', Input=pv_insitu_300x300x300_68vti)
contour4.ContourBy = ['POINTS', 'prs']
contour4.Isosurfaces = [273454990.671875]
contour4.PointMergeMethod = 'Uniform Binning'

# Properties modified on contour4
contour4.ContourBy = ['POINTS', 'v03']
contour4.Isosurfaces = [0.25]

UpdatePipeline(time=0.0, proxy=contour4)

# create a new 'EasyParaViewToABR'
easyParaViewToABR5 = EasyParaViewToABR(registrationName='EasyParaViewToABR5', InputDataset=contour4)

# Properties modified on easyParaViewToABR5
easyParaViewToABR5.a2Dataset = 'Asteroid'
easyParaViewToABR5.a3KeyDataName = 'B31_Asteroid25_60'
easyParaViewToABR5.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR5)