using BaseLib.Abstracts;
using HeartMod.HeartModCode.Extensions;
using Godot;

namespace HeartMod.HeartModCode.Character;

public class HeartModRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => HeartMod.Color;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}