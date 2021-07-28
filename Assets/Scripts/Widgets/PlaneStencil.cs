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

using System;
using UnityEngine;

namespace TiltBrush
{
    public class PlaneStencil : StencilWidget
    {
        private Vector3 m_AspectRatio;
        public override Vector3 Extents
        {
            get
            {
                return m_Size * m_AspectRatio;
            }
            set
            {
                m_Size = 1f;
                m_AspectRatio = value;
                UpdateScale();
                // if (value.x == value.y && value.x == value.z)
                // {
                //     SetSignedWidgetSize(value.x);
                // }
                // else
                // {
                //     throw new ArgumentException("SphereStencil does not support non-uniform extents");
                // }
            }
        }

        // public override Vector3 CustomDimension
        // {
        //     get { return m_AspectRatio; }
        //     set
        //     {
        //         m_AspectRatio = value;
        //         UpdateScale();
        //     }
        // }

        protected override void Awake()
        {
            base.Awake();
            m_Type = StencilType.Plane;
            m_AspectRatio = Vector3.one;
        }

        // Used by
        public override void FindClosestPointOnSurface(Vector3 pos,
                                                       out Vector3 surfacePos, out Vector3 surfaceNorm)
        {
            // Vector3 vCenterToPos = pos - transform.position;
            // float fRadius = Mathf.Abs(GetSignedWidgetSize()) * 0.5f * Coords.CanvasPose.scale;
            // surfacePos = transform.position + vCenterToPos.normalized * fRadius;
            // surfaceNorm = vCenterToPos;

            Vector3 localPos = transform.InverseTransformPoint(pos);
            Vector3 halfWidth = (m_Collider as BoxCollider).size * 0.5f;
            surfacePos.x = Mathf.Clamp(localPos.x, -halfWidth.x, halfWidth.x);
            surfacePos.y = Mathf.Clamp(localPos.y, -halfWidth.y, halfWidth.y);
            surfacePos.z = Mathf.Clamp(localPos.z, -halfWidth.z, halfWidth.z);
            surfacePos.z = halfWidth.z;
            surfaceNorm = -Vector3.forward;

            surfaceNorm = transform.TransformDirection(surfaceNorm);
            surfacePos = transform.TransformPoint(surfacePos);
        }

        // Used to know when you can grab
        override public float GetActivationScore(
            Vector3 vControllerPos, InputManager.ControllerName name)
        {
            float fRadius = Mathf.Abs(GetSignedWidgetSize()) * 0.5f * Coords.CanvasPose.scale;
            float baseScore = (1.0f - (transform.position - vControllerPos).magnitude / fRadius);
            // don't try to scale if invalid; scaling by zero will make it look valid
            if (baseScore < 0) { return baseScore; }
            return baseScore * Mathf.Pow(1 - m_Size / m_MaxSize_CS, 2);
        }

        protected override Axis GetInferredManipulationAxis(
            Vector3 primaryHand, Vector3 secondaryHand, bool secondaryHandInside)
        {
            return Axis.Invalid;
        }

        // public override void RecordAndApplyScaleToAxis(float deltaScale, Axis axis)
        // {
        //     if (m_RecordMovements)
        //     {
        //         Vector3 newDimensions = CustomDimension;
        //         newDimensions[(int)axis] *= deltaScale;
        //         SketchMemoryScript.m_Instance.PerformAndRecordCommand(
        //             new MoveWidgetCommand(this, LocalTransform, newDimensions));
        //     }
        //     else
        //     {
        //         m_AspectRatio[(int)axis] *= deltaScale;
        //         UpdateScale();
        //     }
        // }

        protected override void RegisterHighlightForSpecificAxis(Axis highlightAxis)
        {
            throw new NotImplementedException();
        }

        public override Axis GetScaleAxis(
            Vector3 handA, Vector3 handB,
            out Vector3 axisVec, out float extent)
        {
            // Unexpected -- normally we're only called during a 2-handed manipulation
            Debug.Assert(m_LockedManipulationAxis != null);
            Axis axis = m_LockedManipulationAxis ?? Axis.Invalid;

            // Fill in axisVec, extent
            switch (axis)
            {
                case Axis.Invalid:
                    axisVec = default(Vector3);
                    extent = default(float);
                    break;
                default:
                    throw new NotImplementedException(axis.ToString());
            }

            return axis;
        }

        public override Bounds GetBounds_SelectionCanvasSpace()
        {
            // if (m_Collider != null)
            // {
            //     SphereCollider sphere = m_Collider as SphereCollider;
            //     TrTransform colliderToCanvasXf = App.Scene.SelectionCanvas.Pose.inverse *
            //         TrTransform.FromTransform(m_Collider.transform);
            //     Bounds bounds = new Bounds(colliderToCanvasXf * sphere.center, Vector3.zero);
            //
            //     // Spheres are invariant with rotation, so take out the rotation from the transform and just
            //     // add the two opposing corners.
            //     colliderToCanvasXf.rotation = Quaternion.identity;
            //     bounds.Encapsulate(colliderToCanvasXf * (sphere.center + sphere.radius * Vector3.one));
            //     bounds.Encapsulate(colliderToCanvasXf * (sphere.center - sphere.radius * Vector3.one));
            //
            //     return bounds;
            // }
            // return base.GetBounds_SelectionCanvasSpace();

            if (m_BoxCollider != null)
            {
                TrTransform boxColliderToCanvasXf = App.Scene.SelectionCanvas.Pose.inverse *
                    TrTransform.FromTransform(m_BoxCollider.transform);
                Bounds bounds = new Bounds(boxColliderToCanvasXf * m_BoxCollider.center, Vector3.zero);

                // Transform the corners of the widget bounds into canvas space and extend the total bounds
                // to encapsulate them.
                for (int i = 0; i < 8; i++)
                {
                    bounds.Encapsulate(boxColliderToCanvasXf * (m_BoxCollider.center + Vector3.Scale(
                        m_BoxCollider.size,
                        new Vector3((i & 1) == 0 ? -0.5f : 0.5f,
                            (i & 2) == 0 ? -0.5f : 0.5f,
                            (i & 4) == 0 ? -0.5f : 0.5f))));
                }

                return bounds;
            }
            return new Bounds();
        }

        // override protected void SpoofScaleForShowAnim(float showRatio)
        // {
        //     transform.localScale = m_Size * showRatio * m_AspectRatio;
        // }

        protected override void UpdateScale()
        {
            float maxAspect = m_AspectRatio.Max();
            m_AspectRatio /= maxAspect;
            m_Size *= maxAspect;
            transform.localScale = m_Size * m_AspectRatio;
            UpdateMaterialScale();
        }
    }
} // namespace TiltBrush
