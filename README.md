# Milease

<img src="logo.svg" align="center" width="420">

<p align="center"><b>UI Development · Animation · Productivity</b></p>

> ⚠ The current project is still under development and is subject to significant changes at any time. The documentation is also not yet complete, so please use it with caution.

🎥 Milease is a toolkit aimed at enhancing the Unity UI development experience. It includes more convenient UI animation tools, such as using the Milease Animator to create UI transition animations or using the State Animator for a CSS-like UI design experience. Additionally, Milease contains other UI tools, such as infinite lists and a modified ContentSizeFitter.

Milease is developed by the Morizero team for creating UI tools for their non-commercial rhythm music game, Milthm. At the same time, it maximizes the provision of more general functionalities to benefit the wider community of Unity frontend developers.

**Special Thanks**: 

* https://easings.net

# Introduction

The animation system in Unity is often not well-suited for creating UI animations, and its ease of use in terms of tween editing is relatively limited. Milease integrates tween functions from https://easings.net, allowing you to quickly generate and control them through code. 

# Setup

Unity Editor -> Package Manger -> Add package from git URL...

```
https://github.com/MorizeroDev/Milease.git
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
        transform.Milease(nameof(transform.position),
                new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f),
                1f)
            .While(
                GetComponent<SpriteRenderer>().Milease("color",
                    new Color(1f, 1f, 1f, 1f), new Color(1f, 0f, 0f, 1f),
                    1f, 0.5f)
            )
            .Then(
                transform.Milease(nameof(transform.localScale),
                    new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f),
                    1f, 0f, EaseFunction.Bounce)
            ).Delayed(1f)
            .Then(
                Text.Milease(nameof(Text.text), "Start!", "Finish!", 1f)
            )
            .Then(
                Text.Milease((o, p) =>
                {
                    var text = o as TMP_Text;
                    text.text = $"Hide after {((1f - p) * 2f):F1}s...";
                }, null,2f, 0f, EaseFunction.Linear)
            )
            .Then(
                Text.gameObject.Milease(HandleFunction.Hide, HandleFunction.AutoActiveReset(Text.gameObject), 0f)
            )
            .Then(
                transform.MileaseTo(nameof(transform.position), new Vector3(0f, 0f, 0f), 1f)
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
    Content.GetComponent<TMP_Text>().MilState("color", Color.white),
    Content.MilState(nameof(Content.anchoredPosition), new Vector2(0, 4f), EaseFunction.Back, EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState("color", new Color(0f, 0f, 0f, 0f)),
    GetComponent<Image>().MilState("color", ColorUtils.RGBA(94, 11, 255, 0.5f)),
    Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(110, 0), EaseFunction.Back, EaseType.Out)
};

protected override IEnumerable<MilStateParameter> ConfigHoverState()
    => new[]
{
    Content.GetComponent<TMP_Text>().MilState("color", Color.black),
    Content.MilState(nameof(Content.anchoredPosition), new Vector2(-25, 4f), EaseFunction.Back,
                     EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState("color", new Color(0f, 0f, 0f, 1f)),
    GetComponent<Image>().MilState("color", HoverColor),
    Arrow.MilState(nameof(Content.anchoredPosition), new Vector2(50, 0), EaseFunction.Back, EaseType.Out)
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
            Background.MilState("color", Color.clear),
            Content.rectTransform.MilState(nameof(Content.rectTransform.anchoredPosition), new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState("color", Color.clear),
            Arrow.rectTransform.MilState(nameof(Arrow.rectTransform.anchoredPosition), new Vector2(88f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState("color", Color.black)
        };

    protected override IEnumerable<MilStateParameter> ConfigSelectedState()
        => new[]
        {
            Background.MilState("color", ColorUtils.RGB(0,153,255)),
            Content.rectTransform.MilState(nameof(Content.rectTransform.anchoredPosition), new Vector2(186f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState("color", Color.white),
            Arrow.rectTransform.MilState(nameof(Arrow.rectTransform.anchoredPosition), new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState("color", Color.white)
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

