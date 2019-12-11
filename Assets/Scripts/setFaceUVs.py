

import bpy

# Sets the UVs of all selected faces' vertices to equal their [X,Y] position
# Sets the UVs of all non-selected vertices to [0,0]
def setFaceUVs():
    ob = bpy.context.active_object
    startMode = ob.mode
    polygons = ob.data.polygons
    bpy.ops.object.mode_set(mode = 'OBJECT')
    for f in polygons:
        if f.select:
            for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
                pos = ob.data.vertices[vert_idx].co
                ob.data.uv_layers.active.data[loop_idx].uv = [0.5 - pos.x, 0.5 - pos.y]
        else:
            for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
                ob.data.uv_layers.active.data[loop_idx].uv = [0.5, 0.5]
    bpy.ops.object.mode_set(mode = startMode)






ob = bpy.context.active_object
startMode = ob.mode
polygons = ob.data.polygons
bpy.ops.object.mode_set(mode = 'OBJECT')

for f in polygons:
    if f.select:
        for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
            pos = ob.data.vertices[vert_idx].co
            ob.data.uv_layers.active.data[loop_idx].uv = [0.5 - pos.x, 0.5 - pos.y]
    else:
        for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
            ob.data.uv_layers.active.data[loop_idx].uv = [0.5, 0.5]

bpy.ops.object.mode_set(mode = startMode)
pass



ob = bpy.context.active_object
startMode = ob.mode
polygons = ob.data.polygons
bpy.ops.object.mode_set(mode = 'OBJECT')

for f in polygons:
    if f.normal.z > 0.99:
        for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
            pos = ob.data.vertices[vert_idx].co
            ob.data.uv_layers.active.data[loop_idx].uv = [0.5 - pos.x, 0.5 - pos.y]
    else:
        for vert_idx, loop_idx in zip(f.vertices, f.loop_indices):
            ob.data.uv_layers.active.data[loop_idx].uv = [0.5, 0.5]

bpy.ops.object.mode_set(mode = startMode)
pass

