# Milease

<p align="center"><img src="logo.svg" width="420"></p>

<p align="center"><b>UI Development · Animation · Productivity</b></p>

> [!WARNING] 
>
> The documentation is not yet complete.

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

Or including these in `manifest.json`:

```
"com.morizero.milease": "https://github.com/MorizeroDev/Milease.git",
"party.para.util.colors": "https://github.com/ParaParty/ParaPartyUtil.git?path=Colors",
```

> [!WARNING] 
>
> Due to technical compatibility issues, animation generation statements from v1 (including most **Milease** and **MileaseTo** extension methods) are no longer supported.

# Expression-Driven Animation Updater

Milease compiling animation control code via `Expression` reduces execution time by approximately **90%** compared to using pure reflection.

However, compilation itself can be time-consuming. Fortunately, you can generate a preload script for animation control code by clicking **"Milease -> Generate Milease warming up script"** in the menu. This will compile the control code in advance, ensuring smoother gameplay.

**This method is only applicable to the Mono backend and does not work on IL2CPP.** It is disabled by default, but you can enable it by defining the symbol `MILEASE_ENABLE_EXPRESSION`. 

# Alternative Solution for IL2CPP: Source Code Generation

Due to compatibility and performance issues of `Expression` on IL2CPP, Milease has to use an alternative approach to generate animation code to reduce runtime overhead.

When introducing Milease into your project for the first time, you need to import the "il2cpp support" folder inside the Milease directory. Then, you can generate animation calculation source code via the "Milease" menu options. 

**Additions**:

* You can manually specify types and namespaces to exclude from generation in `GenerationDisableList.cs`.
* You can specify the members for which accessors should be generated in `AccessorGenerationList.cs` to improve runtime efficiency.

# Animate Anything

By a series of extension methods, use Milease anywhere to create and play animations for any field/property! 

Milease supports animation operations for most types such as `Vector3`, `Vector2`, `Color`, `double`, `float`, `int`, `bool`, etc., as long as your type satisfies one of the following conditions:

- It is a Primitive Type
- It is a serializable structure and has overloaded operators: `Type + Type`, `Type - Type`, `Type * float`

For example, use Milease to fade out music:

```c#
AudioSource.MQuadOut(x => x.volume, 1f / 0f.ToThis()).Play();
```

# Create Animations by Organized Code

By using the methods `Milease`, `And`, `Then`, `Delayed`, easily generate complex animations with readable code through nesting:

```c#
var spriteRenderer = GetComponent<SpriteRenderer>();

Animation =
    MilInstantAnimator.Start(
        1f / transform.Milease(x => x.position,new Vector3(0f, 0f, 0f), new Vector3(1f, 1f, 0f))
    )
    .And(
        0.5f + 1f / spriteRenderer.Milease(x => x.color, Color.white, Color.red)
    )
    .Then(
        1f / transform.MBounce(x => transform.localScale, new Vector3(1f, 1f, 1f), new Vector3(2f, 2f, 2f))
    ).Delayed(1f)
    .Then(
        1f / Text.MLinear(x => x.text, "Start!", "Finish!")
    )
    .Then(
        Text.Milease((e) =>
        {
            e.Target.text = $"Hide after {((1f - e.Progress) * 2f):F1}s...";
        }, null, 2f, 0f, EaseFunction.Linear)
    )
    .Then(
        Text.gameObject.Milease(HandleFunction.Hide, HandleFunction.AutoActiveReset(Text.gameObject), 0f)
    )
    .Then(
        1f / transform.Milease(x => x.position, Vector3.zero.ToThis())
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
    Content.GetComponent<TMP_Text>().MilState(x => x.color, Color.white),
    Content.MilState(x => x.anchoredPosition, new Vector2(0, 4f), EaseFunction.Back, EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState(x => x.color, new Color(0f, 0f, 0f, 0f)),
    GetComponent<Image>().MilState(x => x.color, ColorUtils.RGBA(94, 11, 255, 0.5f)),
    Arrow.MilState(x => x.anchoredPosition, new Vector2(110, 0), EaseFunction.Back, EaseType.Out)
};

protected override IEnumerable<MilStateParameter> ConfigHoverState()
    => new[]
{
    Content.GetComponent<TMP_Text>().MilState(x => x.color, Color.black),
    Content.MilState(x => x.anchoredPosition, new Vector2(-25, 4f), EaseFunction.Back,
                     EaseType.Out),
    Arrow.GetComponent<TMP_Text>().MilState(x => x.color, new Color(0f, 0f, 0f, 1f)),
    GetComponent<Image>().MilState(x => x.color, HoverColor),
    Arrow.MilState(x => x.anchoredPosition, new Vector2(50, 0), EaseFunction.Back, EaseType.Out)
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
            Background.MilState(x => x.color, Color.clear),
            Content.rectTransform.MilState(x => x.anchoredPosition, new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState(x => x.color, Color.clear),
            Arrow.rectTransform.MilState(x => x.anchoredPosition, new Vector2(88f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState(x => x.color, Color.black)
        };

    protected override IEnumerable<MilStateParameter> ConfigSelectedState()
        => new[]
        {
            Background.MilState(x => x.color, ColorUtils.RGB(0,153,255)),
            Content.rectTransform.MilState(x => x.anchoredPosition, new Vector2(186f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Arrow.MilState(x => x.color, Color.white),
            Arrow.rectTransform.MilState(x => x.anchoredPosition, new Vector2(138f, 2.3f),
                EaseFunction.Back, EaseType.Out),
            Content.MilState(x => x.color, Color.white)
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

