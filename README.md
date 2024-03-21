# Milease

An advanced UI animation tool with a wide range of easing options to choose from. 

🎥 Perfect for adding dynamic and polished animations to your user interface! 

Current Status:  **WIP**

**Special Thanks**: 

* https://easings.net

# Introduction

The animation system in Unity is often not well-suited for creating UI animations, and its ease of use in terms of tween editing is relatively limited. Milease integrates tween functions from https://easings.net, allowing you to visually design and edit them, as well as quickly generate and control them through code. Additionally, Milease automatically handles tween calculations under vertical sync toggles to ensure smooth animation effects.

# Why Milease is Better

|                   | Milease                                                      | Unity Animator                                               |
| ----------------- | ------------------------------------------------------------ | ------------------------------------------------------------ |
| Smoothness        | 👍 Use `Update` to update animations. UI animations typically do not need to be closely tied to the physics system. We aim to deliver as smooth a UI experience as possible. | 👎 Use `FixedUpdate` to update animations. The default frame rate is 30 frames per second. If you increase the frame rate, it may impose unnecessary burden on the CPU due to its association with physics-related calculations. |
| Creativity        | 👍 Supporting a combination of 34 easing types, simply select from the dropdown menu, making the creation of UI animations more efficient and straightforward. | 👎 The available easing presets are quite limited. Manually adjusting the animation curve is cumbersome, as the vertical axis of the curve is based on animation calculation values rather than easing progress, which is inconvenient in many UI animation design scenarios. |
| Animation Nesting | 👍 You can nest other animations within one animation.        | 👎 While this can be achieved with Timeline, when you have animations attached to both parent and child objects, editing the animation of the parent object may cause inconvenience. Upon clicking the child object to adjust its properties, you'll find that the animation editing interface of the parent object closes, reopening the editing interface of the child object. |
| State Management  | 👍 To better cater to UI animation design, it allows you to choose occasions such as mouse hover or leave to trigger animation playback. | 👎 However, it can be cumbersome, as you need to set up state relationships for each UI animation controller. |
| Reusability       | 👍 Support stacking animations on top with the playback position as the starting point. | 👎 Limited to recording absolute coordinates, resulting in poor reusability for UI animations. |