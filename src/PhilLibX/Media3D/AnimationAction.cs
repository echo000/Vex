using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace PhilLibX.Media3D
{
    /// <summary>
    /// A class to hold an animation action that executes an action during animation playback.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="AnimationAction"/> class.
    /// </remarks>
    /// <param name="name"></param>
    /// <param name="type"></param>
    [DebuggerDisplay("Name = {Name}")]
    public class AnimationAction(string name, string type)
    {
        /// <summary>
        /// Gets or Sets the name of the action.
        /// </summary>
        public string Name { get; set; } = name;

        /// <summary>
        /// Gets or Sets the type of the action.
        /// </summary>
        public string Type { get; set; } = type;

        /// <summary>
        /// Gets or Sets the frames this action occurs at.
        /// </summary>
        public List<AnimationFrame<Action<AnimationAction>?>> Frames { get; set; } = [];
    }
}