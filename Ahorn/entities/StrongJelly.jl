module FrenchHelperStrongJelly
using ..Ahorn, Maple
using Ahorn.FrenchHelper

@mapdef Entity "FrenchHelper/StrongJelly" StrongJelly(x::Integer, y::Integer, jellyColor::String="objects/glider", barrierBehavior::String="AllThrough", bubble::Bool=false, customColor::Bool=true)

const placements = Ahorn.PlacementDict(
    "Strong Jellyfish (French Helper)" => Ahorn.EntityPlacement(
        StrongJelly,
		"point"
    )
)

Ahorn.editingOptions(entity::StrongJelly) = Dict{String, Any}(
    "Directory" => String["objects/glider"],
	"jellyColor" => merge(FrenchHelper.sprites, Dict{String, Any}("Default" => "objects/glider")),
	"barrierBehavior" => merge(FrenchHelper.BarrierBehavior, Dict{String, Any}("Default" => "AllThrough")),
)

function getSprite(entity::StrongJelly)
    s = get(entity.data, "Directory", "objects/glider")
    if (endswith(s, "/")) s = chop(s); end
    sprites = Ahorn.findTextureAnimations(string(s, "/idle"), Ahorn.getAtlas());
    return sprites[1];
end

function Ahorn.selection(entity::StrongJelly)
    x, y = Ahorn.position(entity)

    return Ahorn.getSpriteRectangle(getSprite(entity), x, y)
end

function Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::StrongJelly, room::Maple.Room)

	Ahorn.drawSprite(ctx, getSprite(entity), 0, 0)


    if get(entity.data, "bubble", false)
        curve = Ahorn.SimpleCurve((-7, -1), (7, -1), (0, -6))
        Ahorn.drawSimpleCurve(ctx, curve, (1.0, 1.0, 1.0, 1.0), thickness=1)
    end
end

end
