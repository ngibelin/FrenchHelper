module FrenchHelperFrenchSeekerBarrier
using ..Ahorn, Maple
using Ahorn.FrenchHelper

@mapdef Entity "FrenchHelper/FrenchSeekerBarrier" FrenchSeekerBarrier(x::Integer, y::Integer, width::Integer=Maple.defaultBlockWidth, height::Integer=Maple.defaultBlockHeight,
    color::String="FFFFFF", particleColor::String="FFFFFF", transparency::Number=0.3, particleTransparency::Number=0.5, particleDirection::Number=0.0, wavy::Bool=true)

const placements = Ahorn.PlacementDict(
    "Seeker Barrier (Custom) (French Helper)" => Ahorn.EntityPlacement(
        FrenchSeekerBarrier,
        "rectangle"
    ),
)

Ahorn.editingOptions(entity::FrenchSeekerBarrier) = Dict{String, Any}(
	"color" => merge(FrenchHelper.barrierColor, Dict{String, Any}("Default" => "FFFFFF")),
)

Ahorn.minimumSize(entity::FrenchSeekerBarrier) = 8, 8
Ahorn.resizable(entity::FrenchSeekerBarrier) = true, true

function Ahorn.selection(entity::FrenchSeekerBarrier)
    x, y = Ahorn.position(entity)

    width = Int(get(entity.data, "width", 8))
    height = Int(get(entity.data, "height", 8))

    return Ahorn.Rectangle(x, y, width, height)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::FrenchSeekerBarrier, room::Maple.Room)
    width = Int(get(entity.data, "width", 32))
    height = Int(get(entity.data, "height", 32))

    Ahorn.drawRectangle(ctx, 0, 0, width, height, (0.25, 0.25, 0.25, 0.8), (0.0, 0.0, 0.0, 0.0))
end

end
