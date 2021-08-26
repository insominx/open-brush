// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using UnityEngine;
using UnityEngine.PlayerLoop;

namespace TiltBrush
{

    public class PanelLayoutButton : OptionButton
    {
        [Header("Button textures")]
        [SerializeField] private Texture2D m_BegginerLayoutTexture;
        [SerializeField] private Texture2D m_AdvancedLayoutTexture;
        [SerializeField] private Texture2D m_ClassroomLayoutTexture;

        protected void OnEnable()
        {
            UpdateButtonTexture();
            App.Switchboard.PanelLayoutChanged += UpdateButtonTexture;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            App.Switchboard.PanelLayoutChanged -= UpdateButtonTexture;
        }

        void UpdateButtonTexture()
        {
            if (PanelManager.m_Instance)
            {
                PanelManager.PanelLayout layout = PanelManager.m_Instance.CurrentPanelLayout();
                if (layout == PanelManager.PanelLayout.Beginner)
                {
                    SetButtonTexture(m_BegginerLayoutTexture);
                }
                else if (layout == PanelManager.PanelLayout.Advanced)
                {
                    SetButtonTexture(m_AdvancedLayoutTexture);
                }
                else if (layout == PanelManager.PanelLayout.Classroom)
                {
                    SetButtonTexture(m_ClassroomLayoutTexture);
                }
            }
        }
    }
} // namespace TiltBrush
