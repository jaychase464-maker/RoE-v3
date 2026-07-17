# Temporary Character Asset

`SKM_Character.fbx` is the project owner's locally supplied sample character. It is used only as a replaceable suspect presentation layer.

The raw FBX is ignored by Git to avoid publishing third-party source content. Unity's generated `SKM_Character.fbx.meta` is intentionally allowed so the project can retain a stable asset GUID. Every authorized development machine must place its own licensed copy at this exact path.

The sample contains no embedded textures, facial blendshapes, animation curves, or LODs. The setup tool therefore supplies neutral HDRP materials and a presentation-only procedural Humanoid pose bridge. It does not make the sample production-ready.
