using System.Collections.Generic;
using System.Linq;
using BaseLib.Abstracts;
using HeartMod.HeartModCode.Extensions;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace HeartMod.HeartModCode.Character;

public class HeartModCardPool : CustomCardPoolModel
{
    public override string Title => HeartMod.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


    /* These HSV values will determine the color of your card frames and card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    // #78909C (blue-gray): H≈200°/360≈0.56, S≈0.23, V≈0.61
    public override float H => 0.56f; //Hue; changes the color.
    public override float S => 0.23f; //Saturation
    public override float V => 1f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load HeartMod/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => new("78909C");

    public override bool IsColorless => false;

    // Exclude Token-rarity generated cards (e.g. EmpowerIi/III/IV, ReturnOfTheKing)
    // so they don't appear in card rewards, potions, or other non-combat sources.
  //  public override IEnumerable<CardModel> AllCards => base.AllCards.Where(c => c.Rarity != CardRarity.Token);
}