# Milease

<p align="center"><img src="logo.svg" width="420"></p>

<p align="center"><b>UI Development · Animation · Productivity</b></p>

> ⚠ The documentation is not yet complete.

🎥 Milease is a toolkit aimed at enhancing the Unity UI development experience. It includes more convenient UI animation tools, such as using the Milease Animator to create UI transition animations or using the State Animator for a CSS-like UI design experience. Additionally, Milease contains other UI tools, such as infinite lists and a modified ContentSizeFitter.

Milease is developed by the Morizero team for creating UI tools for their non-commercial rhythm music game, Milthm. At the same time, it maximizes the provision of more general functionalities to benefit the wider community of Unity frontend developers.

**Special Thanks**: 

* https://easings.net

# Introduction

The animation system in Unity is often not well-suited for creating UI animations, and its ease of use in terms of tween editing is relatively limited. Milease integrates tween functions from https://easings.net, allowing you to quickly generate and control them through code. 

# Setup

Unity Editor -> Package Manger -> Add package from git URL...

```
# Milease Core
https://github.com/MorizeroDev/Milease.git

# Color Tools
https://github.com/ParaParty/ParaPartyUtil.git?path=Colors
```

# Animate Anything

By `object.Milease` and `object.MileaseTo`, use Milease anywhere to create and play animations for any field/property! 

Milease supports animation operations for most types such as `Vector3`, `Vector2`, `Color`, `double`, `float`, `int`, `bool`, etc., as long as your type satisfies one of the following conditions:

- It is a Primitive Type
- It is a serializable structure and has overloaded operators: `Type + Type`, `Type - Type`, `Type * float`

For example, use Milease to fade out music:

```c#
AudioSource.MileaseTo(nameof(AudioSource.volume), 0f, 1f, 0f, EaseFunction.Quad, EaseType.Out).Play();
```

# Create Animations by Organized Code

By using the methods `Milease`, `While`, `Then`, `Delayed`, easily generate complex animations with readable code through nesting:

```c#
var animation =
        transform.Milease(UMN.Position,
                new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f),
                1f)
            .While(
                GetComponent<SpriteRenderer>().Milease(UMN.Color,
                    new Color(1f, 1f, 1f, 1f), new Color(1f, 0f, 0f, 1f),
                    1f, 0.5f)
            )
            .Then(
                transform.Milease(UMN.LScale,
                    new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f),
                    1f, 0f, EaseFunction.Bounce)
            ).Delayed(1f)
            .Then(
                Text.Milease(nameof(Text.text), "Start!", "Finish!", 1f)
            )
            .Then(
                Text.Milease((e) =>
                {
                    var text = e.GetTarget<TMP_Text>();
                    text.text = $"Hide after {((1f - e.Progress) * 2f):F1}s...";
                }, null, 2f, 0f, EaseFunction.Linear)
            )
            .Then(
                Text.gameObject.Milease(HandleFunction.Hide, HandleFunction.AutoActiveReset(Text.gameObject), 0f)
            )
            .Then(
                transform.MileaseTo(UMN.Position, new Vector3(0f, 0f, 0f), 1f)
            );
```

# Lightweight Design

Play animations on the fly without the need for pre-prepared animators. With just an animation instance, you can play it anytime, anywhere.

```c#
Animator.Play();
```

# State-based Transition Animator

By utilizing a state-based animator, you can declare various states just like using CSS, and then directly transition animations through the state machine. 

It automatically handles interrupting one state transition halfway through to initiate another state transition, ensuring smooth operation.

```c#
animator.Transition(UIState.Hover);
```

Meanwhile, you can implement the abstract class `MilAnimatedUI` to implement UI more efficiently:

```c#
protected override IEnumerable<MilStateParameter> ConfigDefaultState()
    => new[]
{
    Content.GetComponent<TMP_Text>().MilState(UMN.Color, Color.white),
    Content.MilState(UMN.AnchoredPosition, new Vector2(0, 4f), EaseFunction.Back, EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState(UMN.Color, new Color(0f, 0f, 0f, 0f)),
    GetComponent<Image>().MilState(UMN.Color, ColorUtils.RGBA(94, 11, 255, 0.5f)),
    Arrow.MilState(UMN.AnchoredPosition, new Vector2(110, 0), EaseFunction.Back, EaseType.Out)
};

protected override IEnumerable<MilStateParameter> ConfigHoverState()
    => new[]
{
    Content.GetComponent<TMP_Text>().MilState(UMN.Color, Color.black),
    Content.MilState(UMN.AnchoredPosition, new Vector2(-25, 4f), EaseFunction.Back,
                     EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState(UMN.Color, new Color(0f, 0f, 0f, 1f)),
    GetComponent<Image>().MilState(UMN.Color, HoverColor),
    Arrow.MilState(UMN.AnchoredPosition, new Vector2(50, 0), EaseFunction.Back, EaseType.Out)
};

protected override IEnumerable<MilStateParameter> ConfigSelectedState()
    => null;
```

Of course, you can also dynamically modify states with the following code:

```c#
animator.ModifyState(UIState.Default, Cover, "color", color);
```

# Infinite List

Additionally, Milease encapsulates infinite lists for use, where you only need to create a list item beforehand. For example, here's an implementation of a list item:

```c#
public class TestData
{
    public string Name;
}

public class MilListItemDemo : MilListViewItem
{
    public TMP_Text Content, Arrow;
    public Image Background;

    protected override IEnumerable<MilStateParameter> ConfigDefaultState()
        => new[]
        {
            Background.MilState(UMN.Color, Color.clear),
            Content.rectTransform.MilState(UMN.AnchoredPosition, new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState(UMN.Color, Color.clear),
            Arrow.rectTransform.MilState(UMN.AnchoredPosition, new Vector2(88f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState(UMN.Color, Color.black)
        };

    protected override IEnumerable<MilStateParameter> ConfigSelectedState()
        => new[]
        {
            Background.MilState(UMN.Color, ColorUtils.RGB(0,153,255)),
            Content.rectTransform.MilState(UMN.AnchoredPosition, new Vector2(186f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState(UMN.Color, Color.white),
            Arrow.rectTransform.MilState(UMN.AnchoredPosition, new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState(UMN.Color, Color.white)
        };

    protected override void OnSelect(PointerEventData eventData)
    {

    }

    protected override MilInstantAnimator ConfigClickAnimation()
        => null;

    // Update item appearance by binding data
    // Binding data can be any types
    public override void UpdateAppearance()
    {
        var data = Binding as TestData;
        Content.text = data.Name;
    }

    // Update item appearance by current position in percent
    // This can help you to do some special effects 
    public override void AdjustAppearance(float pos)
    {

    }
}
```

