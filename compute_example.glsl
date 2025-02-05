#[compute]

#version 450

layout(local_size_x = 16, local_size_y = 16, local_size_z = 1) in;

layout(set = 0, binding = 0, std430) buffer VertexBuffer {
    vec3 vertices[];
};

uniform int grid_size;

void main() {
    ivec2 id = ivec2(gl_GlobalInvocationID.xy);
    if (id.x >= grid_size || id.y >= grid_size) return;

    int index = id.y * grid_size + id.x;
    vertices[index] = vec3(id.x, 0.0, id.y);
}
