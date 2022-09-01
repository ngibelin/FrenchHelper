module FrenchHelperTheoBerry

using ..Ahorn, Maple

@mapdef Entity "FrenchHelper/TheoBerry" TheoBerry(x::Integer, y::Integer)

const placements = Ahorn.PlacementDict(
	"Theo Berry (French Helper)" => Ahorn.EntityPlacement(
		TheoBerry
	)
)

sprite = "collectables/FrenchHelper/TheoBerry/idle00"

function Ahorn.selection(entity::TheoBerry)
    x, y = Ahorn.position(entity)
    return Ahorn.getSpriteRectangle(sprite, x, y)
end

Ahorn.render(ctx::Ahorn.Cairo.CairoContext, entity::TheoBerry, room::Maple.Room) = Ahorn.drawSprite(ctx, sprite, 0, 0)

function Ahorn.renderSelectedAbs(ctx::Ahorn.Cairo.CairoContext, entity::TheoBerry)
	x, y = Ahorn.position(entity)

	for node in get(entity.data, "nodes", ())
		nx, ny = node

		Ahorn.drawLines(ctx, Tuple{Number, Number}[(x, y), (nx, ny)], Ahorn.colors.selection_selected_fc)
	end
end

function Ahorn.renderAbs(ctx::Ahorn.Cairo.CairoContext, entity::TheoBerry, room::Maple.Room)
	x, y = Ahorn.position(entity)

	Ahorn.drawSprite(ctx, sprite, x, y)
end

end