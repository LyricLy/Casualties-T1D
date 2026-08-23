# Type 1 Diabetes (T1D)

Makes the experiment type 1 diabetic. You will have to manage your blood glucose and ketones with insulin therapy.

## Disclaimer
The mechanics of this mod are not fully realistic. Liberties have been taken to make the gameplay simpler and make it easier for the player to treat themselves without help.
**Do not** use any techniques from the mod on a real person with diabetes if you are not knowledgable about the treatment of diabetes in real life.

Diabetic people can normally take care of themselves, but if they are very confused, unresponsive, or unconscious, they need help: call your country's emergency number immediately and follow their directions.
Don't wait. Do not give them **any** medication, **especially not insulin**, unless the dispatcher tells you to. Do not feed them anything if they're not alert enough to eat on their own.
They may carry a glucagon spray or injection for hypoglycemia; the dispatcher will tell you how and when to use it.

## Overview

T1D adds a new persistent threat to the player: your blood sugar level, which is constantly rising and must be kept down with insulin. The consumption of carbohydrates will rise it further, requiring you to use
more of your limited supply to compensate for having tasty food. High blood sugar will dehydrate you and slow wound healing.
If you're without insulin for too long, ketone bodies will build up and begin to [make your blood more acidic](https://en.wikipedia.org/wiki/Diabetic_ketoacidosis), with terrible consequences for your body.
But if you're not careful and take too much, your blood sugar can drop too low, leaving your cells without energy. That can go very wrong, very quickly...

This mod adds:
- A detailed simulation of type 1 diabetes mellitus, including the effects of low and high blood sugar and ketoacidosis, as well as the mechanics of insulin and glucagon
- 8 new items and 4 liquids for the management of diabetes, including craftable insulin and medical devices
- 5 custom run settings, complete with default values for the base presets
- Difficulty! Living with diabetes is hard and you'll have to learn the mechanics of this mod from the ground up. Failure to engage with the mechanics correctly will result in a quick death.

## Author's note

I really like how this game conveys the feeling of being in dire straits: starving, thirsty, bleeding out, desperately trying to hold on long enough to fix what's wrong. I also like how, while simplified,
it can serve as a platform to teach people a bit about the biology of the human body. I hope this mod can convey some of what it feels like to have type 1 diabetes. The constant management of your blood sugar,
the consequences to your health and mood when you get it wrong, the effects of hypoglycemia and ketoacidosis, and the amount that insulin pumps and continuous glucose monitors can help. I've experienced all of it
and more.

This mod shows a breadth of ways to manage T1D, all of which are based on real life. People use all of these methods, often solely due to cost. Pens that let you precisely control your dose are more expensive
than a syringe. A pump is more expensive still, both to get the gadget itself and to afford all the consumables you need in perpetuity. Getting it to track your glucose constantly and adjust accordingly is yet
more money for a fancier pump and even more consumables. Many people can't access insulin itself, let alone all these bells and whistles.

Thousands of people die of type 1 diabetes every year. Many of these deaths are preventable: they die because they can't afford insulin or because their country lacks the infrastructure to care for them.
Some die of diabetes before even being diagnosed. Everything that happens to you in this mod is happening to hundreds of people worldwide at the same time. I'm very lucky to have the resources I have to treat
my disease, and it's still difficult for me. Many people aren't nearly as lucky as me.

## On the implementation

I had to take some liberties to turn this real disease into a game, as you do. Here are some notes on that for the benefit of fellow T1s who find it interesting:
- Insulin is 200x less effective. 1ml would normally be 100 units; I've made it more like 0.5 units. Syringes are normally 100ml, and as the game rounds liquids to the nearest millilitre, potency has to be turned down
  or managing the insulin you have would get really annoying. Doses are 2ml so that insulin pens aren't overpowered for injecting pure fentanyl. You don't get many units at a time in a cartridge to keep it as a
  scarce resource.
- ICR and ISF are fixed and configurable as run settings (10 and 2.0 by default), though I do like the idea of them being a bit random so you have to get a feel for it while playing each run! Might come back to that...
- Basal needs are basically completely flat (glycogenolysis/gluconeogenesis occur at a constant rate), though insulin sensitivity does change according to bodily state (e.g. more effective with exercise, less when sick).
  - I worried this might make management too simple but it still felt pretty tricky and familiar to me. I'm pretty happy with the balance of how it feels to go low, go high, and control BG over time.
    It might be on the micro-managey side for a game, but if someone complains I can always go "now try being me and having to do it all the time"...
- Metformin is included although it's normally a T2 medication. It actually works for us too! It isn't normally given because we don't have as big of an issue with insulin resistance, but it has been shown to
  reduce insulin requirements in T1.
- Metformin overdose gives you ketones. This isn't correct (it should be lactic acid) but as they're both acids and the only treatment for acidosis in this mod is to wait for it to clear, I figured it doesn't really
  matter what kind of acid it is.
- Most item sprites are based directly on real products I've used. Recognize any of them?
- If you're American, 1 mmol/l is about 18 mg/dl.
- The experiment is in the early stages of the disease and still has some pancreatic function (a bit of insulin/glucagon production). I took this liberty to make it easier to control levels thanks to a slight push
  toward the 4.0-6.0 mark. It also serves as an abstraction of other factors that affect blood glucose and makes the glucose level "bounce around" a bit, so it's harder to predict and doesn't seem completely linear.
- Ketone levels and blood acidity are conflated for simplicity. In reality, ketones alone don't necessarily cause acidosis if there is sufficient bicarbonate to keep the pH stable.
  (That's the difference between DKA and a keto diet.) Experiments are also a little more sensitive to acidosis, so 5.0 mmol/l is nigh-unsurvivable for them, though humans have survived levels well beyond that.
- Related to the above point, ketones' role as an alternative energy source is entirely missing, making them purely negative. Maybe a mutation along the line in the Experiments' creation rendered them unable to utilize them?

## Compatibility

The conviction with which I guarantee compatibility depends on the kind of mod:
- Multiplayer is supported in v4.0.1 of Casualties: Together. It will break soon when v5 comes out, but I intend to eventually support that version as well. I take responsibility for how this mod works with
  multiplayer and will try to support it as best as I can. Please report any issues with it to me.
- The mod *should* work fine with unrelated mods that don't affect the health panel. Let me know if there's a conflict: I'll take a look and might fix it (especially if it's my fault). No guarantees.
- Conflicts are likely to occur with other mods that add information to the health panel or change how it looks, because this mod adds info to it.
  Don't bother reporting this unless you're the author of the mod and want to help improve support. I'm very unlikely to fix these issues.
- Note for later: integration with Casualty Vitals to make insulin lower potassium would be funny...
