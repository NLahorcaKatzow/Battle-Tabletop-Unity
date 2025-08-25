Included Shaders

1. Basic
A versatile shader that allows you to control:

Light, Mid, and Dark color tones

Metallic and Roughness

Normal Map intensity

Emissive color and strength

Ideal for hand-painted textures or toon-style surfaces with color control directly in the material.

2. MixTextures
Similar to the Basic shader, but instead of color controls, it uses:

Base Color, Metallic, and Roughness textures.

Great for physically-based workflows where texture maps define surface properties.

3. WorldAligned
A triplanar-mapped version of the Basic shader.

Provides:

Light/Mid/Dark color control

Triplanar projection for seamless mapping on 3D models

Support for Normal Map, Metallic, Roughness, and Emissive

4. WA_Mixed (World Aligned Mixed)
A triplanar version of MixTextures:

Uses Base Color, Metallic, and Roughness textures

Triplanar projection (no visible seams on UVs)

Includes Normal Map and Emissive support

