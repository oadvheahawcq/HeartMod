using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using HeartMod.HeartModCode.Character;
using HeartMod.HeartModCode.Extensions;

namespace HeartMod.HeartModCode.Potions;

[Pool(typeof(HeartModPotionPool))]
public abstract class HeartModPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".PotionOutlinePath();
}
