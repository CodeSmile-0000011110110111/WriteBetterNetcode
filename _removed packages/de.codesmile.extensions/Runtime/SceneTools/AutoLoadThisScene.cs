// Copyright (C) 2021-2024 Steffen Itterheim
// Refer to included LICENSE file for terms and conditions.

using CodeSmile.Components;

namespace CodeSmile.SceneTools
{
	internal sealed class AutoLoadThisScene : OneTimeTaskBehaviour
	{
		private void Awake() => Destroy(this);
		private void Reset() => TryAddActiveScene();
		private void OnValidate() => TryAddActiveScene();

		private void TryAddActiveScene()
		{
#if UNITY_EDITOR
			var runtimeLoad = AutoLoadScenes.Instance;
			if (runtimeLoad != null)
			{
				if (enabled && gameObject.activeInHierarchy)
					runtimeLoad.AddScene(gameObject.scene);
				else
				{
					runtimeLoad.RemoveScene(gameObject.scene);
				}
			}
#endif
		}
	}
}
