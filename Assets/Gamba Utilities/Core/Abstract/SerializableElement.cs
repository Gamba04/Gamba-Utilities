using System;
using System.Collections.Generic;
using UnityEngine;

namespace GambaUtilities
{
	/// <summary> Base class for serializable classes which are meant to be elements of a serialized <see cref="List{T}"/> or <see cref="Array"/>. </summary>
	/// <remarks>
	///		Features:
	///		<list type="bullet">
	///			<item>
	///				<see cref="name"/> is a custom programmable name for the displayed elements in the inspector.<br/>
	///				You can update its value using an <see langword="OnValidate"/> method in the container class.
	///			</item>
	///			<item>
	///				<see cref="Init"/> is an optional initialization method to set the contents of the first serialized elements.<br/>
	///				Unity's built-in behaviour overrides the configured defaults for new elements created from the inspector.
	///			</item>
	///		</list>
	/// </remarks>
	public abstract class SerializableElement : ISerializationCallbackReceiver
	{
		/// <summary> Name to be displayed for the element in the inspector. </summary>
		[SerializeField, HideInInspector]
		protected string name;
		[SerializeField, HideInInspector]
		private bool initialized;

		#region ISerializationCallbackReceiver

		void ISerializationCallbackReceiver.OnBeforeSerialize() { }

		void ISerializationCallbackReceiver.OnAfterDeserialize()
		{
			if (!initialized)
			{
				initialized = true;

				Init();
			}
		}

		#endregion

		// ----------------------------------------------------------------------------------------------------

		#region Init

		/// <summary> Custom initialization called upon the first elements after being added from the inspector. </summary>
		public virtual void Init() { }

		#endregion

	}
}