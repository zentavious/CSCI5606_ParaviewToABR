# trace generated using paraview version 5.11.0
#import paraview
#paraview.compatibility.major = 5
#paraview.compatibility.minor = 11

#### import the simple module from the paraview
from paraview.simple import *
#### disable automatic camera reset on 'Show'
paraview.simple._DisableFirstRenderCameraReset()

# create a new 'XML Image Data Reader'
pv_insitu_300x300x300_00000vti = XMLImageDataReader(registrationName='pv_insitu_300x300x300_00000.vti', FileName=['C:\\Users\\Zenta_t2ma3ok\\Documents\\Visualization\\ProjectData\\B31\\pv_insitu_300x300x300_00000.vti'])
pv_insitu_300x300x300_00000vti.CellArrayStatus = ['vtkGhostType']
pv_insitu_300x300x300_00000vti.PointArrayStatus = ['prs', 'tev', 'v02', 'v03', 'vtkValidPointMask', 'vtkGhostType']

# Properties modified on pv_insitu_300x300x300_00000vti
pv_insitu_300x300x300_00000vti.TimeArray = 'None'

UpdatePipeline(time=0.0, proxy=pv_insitu_300x300x300_00000vti)

# create a new 'Resample To Image'
resampleToImage1 = ResampleToImage(registrationName='ResampleToImage1', Input=pv_insitu_300x300x300_00000vti)
resampleToImage1.SamplingBounds = [-2300000.0, 2300000.0001149997, -500000.0, 2300000.000005, -1200000.0, 1199999.9999872]

UpdatePipeline(time=0.0, proxy=resampleToImage1)

# load plugin
# LoadPlugin('C:\Users\Zenta_t2ma3ok\EasyParaViewToABR\EasyParaViewToABR.py', remote=False, ns=globals())

# create a new 'EasyParaViewToABR'
easyParaViewToABR1 = EasyParaViewToABR(registrationName='EasyParaViewToABR1', InputDataset=resampleToImage1)

# Properties modified on easyParaViewToABR1
easyParaViewToABR1.a2Dataset = 'Asteroid'
easyParaViewToABR1.a3KeyDataName = 'B31_Volume_0'
easyParaViewToABR1.a1Organization = 'LANL'

UpdatePipeline(time=0.0, proxy=easyParaViewToABR1)