module FrenchHelperJellyBlock
using ..Ahorn, Maple
using Ahorn.FrenchHelper

@mapdef Entity "FrenchHelper/JellyBlock" JellyBlock(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight, color::String="normal")

const placements = Ahorn.PlacementDict(
    "Jelly Block (French Helper)" => Ahorn.EntityPlacement(
        JellyBlock,
        "rectangle"
    )
)

Ahorn.editingOptions(entity::JellyBlock) = Dict{String, Any}(
	"color" => merge(FrenchHelper.jellyBlockSprites, Dict{String, Any}("Default" => "normal")),
)

Ahorn.minimumSize(entity::JellyBlock) = 8, 8
Ahorn.resizable(entity::JellyBlock) = true, true

Ahorn.selection(entity::JellyBlock) = Ahorn.getEntityRectangle(entity)

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::JellyBlock, room::Maple.Room)
    x = Int(get(entity.data, "x", 0))
    y = Int(get(entity.data, "y", 0))

    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (1.0, 1.0, 1.0, 0.5), (1.0, 1.0, 1.0, 0.5))
end

end
