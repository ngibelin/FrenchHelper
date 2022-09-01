module FrenchHelper
using ..Ahorn, Maple

const BarrierBehavior = Dict{String, String}(
    "AllThrough" => "AllThrough",
    "ColorCoded" => "ColorCoded",
	"ColorCollide" => "ColorCollide",
    "Vanilla" => "Vanilla"
)

const sprites = Dict{String, String}(
    "Deep blue" => "objects/FrenchHelper/StrongJelly/deep_blue",
	"Green" => "objects/FrenchHelper/StrongJelly/green",
	"Grey" => "objects/FrenchHelper/StrongJelly/grey",
	"Orange" => "objects/FrenchHelper/StrongJelly/orange",
	"Pink" => "objects/FrenchHelper/StrongJelly/pink",
	"Purple" => "objects/FrenchHelper/StrongJelly/purple",
	"Red" => "objects/FrenchHelper/StrongJelly/red",
	"Yellow" => "objects/FrenchHelper/StrongJelly/yellow",
)

const jellyBlockSprites = Dict{String, String}(
	"Deep blue" => "deep_blue",
	"Green" => "green",
	"Grey" => "grey",
	"Orange" => "orange",
	"Pink" => "pink",
	"Purple" => "purple",
	"Red" => "red",
	"Yellow" => "yellow",
)

const barrierColor = Dict{String, String}(
	"Deep blue" => "5555FF",
	"Green" => "71C837",
	"Grey" => "808080",
	"Orange" => "FF7F2A",
	"Pink" => "D42AFF",
	"Purple" => "7F2AFF",
	"Red" => "FF2A2A",
	"Yellow" => "FFD42A",
)

end