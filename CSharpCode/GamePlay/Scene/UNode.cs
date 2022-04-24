﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS.GamePlay.Scene
{
    public class UNodeAttribute : Attribute
    {
        public System.Type NodeDataType = typeof(UNodeData);
        public string DefaultNamePrefix = "Node";
    }
    public class UNodeData : IO.BaseSerializer
    {
        public int GetStructSize()
        {
            //return System.Runtime.InteropServices.Marshal.SizeOf(this.GetType());
            return 1024;
        }
        [Rtti.Meta]
        public UNode.ENodeStyles NodeStyles { get; set; } = 0;
        [Rtti.Meta(Order = 0)]
        public string Name { get; set; }
        UBoundVolume mBoundVolume;
        [Rtti.Meta(Order = 1)]
        public UBoundVolume BoundVolume
        {
            get => mBoundVolume;
            set => mBoundVolume = value;
        }
        UPlacementBase mPlacement = null;
        [Rtti.Meta(Order = 2)]
        public UPlacementBase Placement
        {
            get => mPlacement;
            set
            {
                mPlacement = value;
            }
        }

        public bool HasStyle(UNode.ENodeStyles style)
        {
            return (NodeStyles & style) == style;
        }
        public void SetStyle(UNode.ENodeStyles style)
        {
            NodeStyles |= style;
        }
        public void UnsetStyle(UNode.ENodeStyles style)
        {
            NodeStyles &= ~style;
        }
    }
    public partial class UNode
    {
        public virtual async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)
        {
            NodeData = data;

            if (NodeData != null)
            {
                if (NodeData.Placement == null && placementType != null)
                {
                    var args = new object[] { this };
                    NodeData.Placement = Rtti.UTypeDescManager.CreateInstance(placementType, args) as UPlacementBase;
                    switch (bvType)
                    {
                        case EBoundVolumeType.None:
                            {
                                NodeData.BoundVolume = null;
                            }
                            break;
                        case EBoundVolumeType.Box:
                            {
                                var boxBV = new UBoxBV(this);
                                boxBV.LocalAABB = new BoundingBox(Vector3.Zero);
                                NodeData.BoundVolume = boxBV;
                            }
                            break;
                        case EBoundVolumeType.Sphere:
                            {
                                var sphereBV = new USphereBV(this);
                                sphereBV.Center = Vector3.Zero;
                                sphereBV.Radius = 1;
                                NodeData.BoundVolume = sphereBV;
                            }
                            break;
                    }

                    Placement.SetTransform(in FTransform.Identity);
                }
                UpdateAABB();
                UpdateAbsTransform();
            }

            return true;
        }
        public void GetWorldSpaceBoundingBox(out DBoundingBox outVal)
        {
            //var matrix = this.Placement.AbsTransform.ToMatrixNoScale();
            //BoundingBox.Transform(in AABB, in matrix, out outVal);            
            DBoundingBox.TransformNoScale(in AABB, in Placement.AbsTransform, out outVal);
        }
        [Flags]
        public enum ENodeStyles
        {
            VisibleMeshProvider = (1 << 0),//deprecated
            VisibleFollowParent = (1 << 1),
            HitproxyMasks = (1 << 2) | (1 << 3),//value: 0,1,2 NoProxy,RootProxy,FollowProxy
            DiscardAABB = (1<<4),
            CastShadow = (1 << 5),
            AcceptShadow = (1 << 6),
            HideBoundShape = (1 << 7),
            NoPickedDraw = (1 << 8),
            SelfInvisible = (1 << 9),
            ChildrenInvisible = (1 << 10),
            SceneManaged = (1<<11),
            Invisible = SelfInvisible | ChildrenInvisible,
        }
        public ENodeStyles NodeStyles 
        {
            get
            {
                if (NodeData == null)
                    return (ENodeStyles)0;
                return NodeData.NodeStyles;
            }
            set
            {
                if (NodeData != null)
                {
                    NodeData.NodeStyles = value;
                }
            }
        }
        #region Styles
        public bool HasStyle(ENodeStyles style)
        {
            return (NodeStyles & style) == style;
        }
        public void SetStyle(ENodeStyles style)
        {
            NodeStyles |= style;
        }
        public void UnsetStyle(ENodeStyles style)
        {
            NodeStyles &= ~style;
        }
        public virtual bool IsCastShadow
        {
            get
            {
                return HasStyle(ENodeStyles.CastShadow);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.CastShadow);
                }
                else
                {
                    UnsetStyle(ENodeStyles.CastShadow);
                }
            }
        }
        public virtual bool IsAcceptShadow
        {
            get
            {
                return HasStyle(ENodeStyles.AcceptShadow);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.AcceptShadow);
                }
                else
                {
                    UnsetStyle(ENodeStyles.AcceptShadow);
                }
            }
        }
        public virtual bool IsSceneManaged
        {
            get
            {
                return HasStyle(ENodeStyles.SceneManaged);
            }
            set
            {
                if (value)
                {
                    SetStyle(ENodeStyles.SceneManaged);
                    if(this.SceneId == UInt32.MaxValue)
                    {
                        ParentScene?.AllocId(this);
                    }
                }
                else
                {
                    UnsetStyle(ENodeStyles.SceneManaged);
                    if (this.SceneId != UInt32.MaxValue)
                    {
                        ParentScene?.FreeId(this);
                    }
                }
            }
        }
        #endregion
        
        #region BaseFields
        public virtual bool IsSceneManagedType()
        {
            return false;
        }
        
        public virtual UInt32 SceneId
        {
            get => UInt32.MaxValue;
            internal set { }
        }
        public virtual string NodeName
        {
            get
            {
                if (mNodeData != null)
                {
                    return mNodeData.Name;
                }
                return SceneId.ToString();
            }
        }
        protected UNode mParent;
        public virtual UNode Parent
        {
            get => mParent;
            set
            {
                if (mParent == value)
                    return;
                var oldScene = GetNearestParentScene();

                if (mParent != null)
                {
                    mParent.Children.Remove(this);
                }                
                mParent = value;
                if (mParent != null)
                {
                    if (mParent.Children.Contains(this) == false)
                        mParent.Children.Add(this);
                }

                var newScene = GetNearestParentScene();
                if (oldScene != newScene)
                {
                    if (IsSceneManagedType() && IsSceneManaged)
                    {
                        oldScene?.FreeId(this);
                        newScene?.AllocId(this);
                    }
                    ParentSceneChanged(oldScene, newScene);
                }

                if (Placement != null)
                {
                    Placement.Position = Placement.Position;
                }
            }
        }
        public UScene ParentScene
        {
            get
            {
                return GetNearestParentScene();
            }
        }
        public UWorld GetWorld()
        {
            var scene = ParentScene;
            if (scene != null)
                return scene.World;
            return null;
        }
        public List<UNode> Children { get; } = new List<UNode>();
        UNodeData mNodeData = null;
        public UNodeData NodeData
        {
            get => mNodeData;
            set
            {
                mNodeData = value;
                if (Placement != null)
                {
                    UpdateAABB();
                    UpdateAbsTransform();
                }
            }
        }
        public DVector3 Location
        {
            get
            {
                if (Placement == null)
                    return DVector3.Zero;
                return Placement.AbsTransform.Position;
            }
        }
        public UPlacementBase Placement
        {
            get { return NodeData?.Placement; }
        }
        public UBoundVolume BoundVolume
        {
            get { return NodeData?.BoundVolume; }
        }
        public DBoundingBox AABB;//包含Child的AABB
        #endregion

        #region Virtual Interface
        private void ParentSceneChanged(UScene prev, UScene cur)
        {
            OnParentSceneChanged(prev, cur);
            foreach(var i in Children)
            {
                i.ParentSceneChanged(prev, cur);
            }
        }
        protected virtual void OnParentSceneChanged(UScene prev, UScene cur)
        {

        }
        public virtual void OnGatherVisibleMeshes(UWorld.UVisParameter rp)
        {
            if (rp.VisibleNodes != null)
            {
                rp.VisibleNodes.Add(this);
            }
        }
        #endregion

        #region Link
        private void UnsafeNullParent()
        {
            ParentScene?.FreeId(this);
            mParent = null;
        }
        public void ClearChildren()
        {
            foreach(var i in Children)
            {
                i.UnsafeNullParent();
            }
            Children.Clear();
            UpdateAABB();
        }
        public UNode FindFirstChild(string name)
        {
            foreach (var i in Children)
            {
                if (i.NodeName == name)
                    return i;
            }
            return null;
        }
        public UScene GetNearestParentScene()
        {
            if (GetType() == typeof(UScene) || GetType().IsSubclassOf(typeof(UScene)))
                return this as UScene;
            if (mParent != null)
                return mParent.GetNearestParentScene();
            else
                return null;
        }
        public delegate bool FOnVisitNode(UNode node, object arg);
        public bool DFS_VisitNodeTree(FOnVisitNode visitor, object arg)
        {
            if (!HasStyle(ENodeStyles.SelfInvisible) && visitor(this, arg))
            {
                return true;
            }
            if (!HasStyle(ENodeStyles.ChildrenInvisible))
            {
                foreach (var i in Children)
                {
                    if (i.DFS_VisitNodeTree(visitor, arg))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region Save&Load
        public const uint NodeDescAttributeFlags = 1;
        public unsafe void SaveChildNode(UScene scene, EngineNS.XndHolder xnd, EngineNS.XndNode node)
        {
            foreach(var i in Children)
            {
                var typeStr = Rtti.UTypeDesc.TypeStr(i.GetType());
                using (var nd = xnd.NewNode(typeStr, 1, 0))
                {
                    node.AddNode(nd);

                    if (i.NodeData != null)
                    {
                        using (var dataAttr = xnd.NewAttribute(Rtti.UTypeDesc.TypeStr(i.NodeData.GetType()), 1, NodeDescAttributeFlags))
                        {
                            var attrProxy = new EngineNS.IO.XndAttributeWriter(dataAttr);

                            var ar = new EngineNS.IO.AuxWriter<EngineNS.IO.XndAttributeWriter>(attrProxy);
                            dataAttr.BeginWrite((ulong)NodeData.GetStructSize() * 2);
                            ar.Write(i.NodeData);
                            dataAttr.EndWrite();

                            nd.AddAttribute(dataAttr);
                        }   
                    }
                    i.SaveChildNode(scene, xnd, nd);
                }   
            }
        }
        public async System.Threading.Tasks.Task<bool> LoadChildNode(GamePlay.UWorld world, UScene scene, EngineNS.XndNode node)
        {
            for(uint i = 0; i < node.GetNumOfNode(); i++)
            {
                var cld = node.GetNode(i);
                var cldTypeStr = cld.NativeSuper.Name;
                
                var attr = cld.FindFirstAttributeByFlags(NodeDescAttributeFlags);
                if (attr.NativePointer == IntPtr.Zero)
                {
                    continue;
                }
                UNodeData nodeData = Rtti.UTypeDescManager.CreateInstance(Rtti.UTypeDesc.TypeOf(attr.Name)) as UNodeData;
                var nd = await scene.NewNode(world, cldTypeStr, nodeData, EBoundVolumeType.None, null);
                var ar = attr.GetReader(scene);
                IO.ISerializer data = nodeData;
                try
                {
                    ar.Tag = nd;
                    ar.ReadTo(data, this);
                    nd.NodeData = data as UNodeData;

                    nd.OnNodeLoaded();
                }
                catch (Exception ex)
                {
                    Profiler.Log.WriteException(ex);
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "IO", $"Scene({scene.AssetName}): Node({nd.NodeData?.Name}) load failed");
                }
                ar.Tag = null;
                attr.ReleaseReader(ref ar);

                nd.Parent = this;
                await nd.LoadChildNode(world, scene, cld);
            }
            return true;
        }
        public virtual void OnNodeLoaded()
        {
            
        }
        public virtual void OnSceneLoaded()
        {
            this.HitproxyType = this.HitproxyType;
        }
        #endregion

        #region Hierarchical Collide
        protected virtual void OnAbsTransformChanged()
        {

        }
        public void UpdateAbsTransform()
        {
            if (NodeData == null || Placement == null)
                return;
            if (Parent == null)
            {
                Placement.AbsTransform = Placement.TransformData;
                //var matrix = Placement.AbsTransform.ToMatrixNoScale();
                //Placement.AbsTransformInv = Matrix.Invert(in matrix);
                OnAbsTransformChanged();
            }
            else
            {
                if(Placement.IsIdentity)
                {
                    Placement.AbsTransform = Parent.Placement.AbsTransform;
                }
                else
                {
                    var transform = Placement.TransformData;
                    if (Parent.Placement.InheritScale)
                    {
                        FTransform.Multiply(out Placement.AbsTransform, in transform, in Parent.Placement.AbsTransform);
                    }
                    else
                    {
                        FTransform.MultiplyNoParentScale(out Placement.AbsTransform, in transform, in Parent.Placement.AbsTransform);
                    }
                    //Placement.AbsTransform = Placement.TransformData Transform * Parent.Placement.AbsTransform;
                }
                //var matrix = Placement.AbsTransform.ToMatrixNoScale();
                //Placement.AbsTransformInv = Matrix.Invert(in matrix);
                OnAbsTransformChanged();
            }
            UpdateChildrenAbsTransform();
        }
        public void UpdateChildrenAbsTransform()
        {
            foreach (var i in Children)
            {
                i.UpdateAbsTransform();
            }
        }
        public void UpdateAABB()
        {
            if (NodeData == null || BoundVolume == null || Placement == null)
                return;
            if (BoundVolume != null && BoundVolume.mLocalAABB.IsEmpty() == false && HasStyle(ENodeStyles.DiscardAABB) == false)
            {
                //BoundingBox.Transform(ref BoundVolume.mLocalAABB, ref Placement.AbsTransform, out AABB);
                AABB.FromSingle(in BoundVolume.mLocalAABB);
                AABB.Maximum = AABB.Maximum * Placement.Scale;
                AABB.Minimum = AABB.Minimum * Placement.Scale;
            }
            else
            {
                AABB.InitEmptyBox();
            }
            foreach (var i in Children)
            {
                if (i.HasStyle(ENodeStyles.DiscardAABB))
                    continue;
                if (i.Placement.IsIdentity == false)
                {
                    var uplc = i.Placement as UPlacement;
                    if (uplc != null)
                    {
                        DBoundingBox tmp;
                        //var matrix = uplc.TransformData.ToMatrixNoScale();
                        var matrix = uplc.TransformData;
                        DBoundingBox.TransformNoScale(in i.AABB, in matrix, out tmp);
                        //BoundingBox.Transform(in i.AABB, in uplc.mTransform, out tmp);
                        AABB = DBoundingBox.Merge(AABB, tmp);
                    }
                    else
                    {
                        DBoundingBox tmp;
                        //var trans = i.Placement.TransformData;
                        //var trans = i.Placement.TransformData.ToMatrixNoScale();
                        var trans = i.Placement.TransformData;
                        DBoundingBox.TransformNoScale(in i.AABB, in trans, out tmp);
                        AABB = DBoundingBox.Merge(AABB, tmp);
                    }
                }
                else
                {
                    AABB = DBoundingBox.Merge(AABB, i.AABB);
                }
            }
            if (Parent != null)
            {
                Parent.UpdateAABB();
            }
        }
        public bool LineCheck(in DVector3 start, in DVector3 end, ref VHitResult result)
        {
            float Near, Far;
            var vNear = new Vector3();
            var vFar = new Vector3();
            var localStart = Placement.AbsTransform.InverseTransformPositionNoScale(in start).ToSingleVector3();
            var localEnd = Placement.AbsTransform.InverseTransformPositionNoScale(in end).ToSingleVector3();
            //var localStart = Vector3.TransformCoordinate(start, Placement.AbsTransformInv);
            //var localEnd = Vector3.TransformCoordinate(end, Placement.AbsTransformInv);
            //if (!Vector3.Equals(in localStart1, in localStart, 0.001f) ||
            //    !Vector3.Equals(in localEnd1, in localEnd, 0.001f))
            //{
            //    int xxx = 0;
            //}
            unsafe
            {
                var aabb = AABB.ToSingleAABB();
                //fixed (BoundingBox* pBox = &AABB)
                {
                    BoundingBox* pBox = &aabb;
                    var dir = localEnd - localStart;
                    if (/*AABB.IsEmpty()==false && */IDllImportApi.v3dxLineIntersectBox3(&Near, &vNear, &Far, &vFar, &localStart, &dir, pBox) == 0)
                    {
                        return false;
                    }
                    if (OnLineCheckTriangle(in localStart, in localEnd, ref result))
                    {
                        result.Position = Placement.AbsTransform.TransformPositionNoScale(result.m_Position.AsDVector()).ToSingleVector3();
                        result.Normal = Placement.AbsTransform.TransformVector3(in result.m_Normal);
                        //result.Position = Vector3.TransformCoordinate(result.Position, Placement.AbsTransform);
                        //result.Normal = Vector3.TransformNormal(result.Normal, Placement.AbsTransform);
                        return true;
                    }
                    else
                    {
                        foreach (var i in Children)
                        {
                            if (i.LineCheck(in start, in end, ref result) == true)
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                }
            }
        }
        public virtual bool OnLineCheckTriangle(in Vector3 start, in Vector3 end, ref VHitResult result)
        {
            //todo: perface test            
            return false;
        }
        #endregion

        #region GamePlay
        public virtual void TickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            if (OnTickLogic(world, policy) == false)
                return;

            //foreach(var i in Children)
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].TickLogic(world, policy);
            }
        }
        public virtual bool OnTickLogic(GamePlay.UWorld world, Graphics.Pipeline.URenderPolicy policy)
        {
            return true;
        }
        #endregion
    }
    public partial class USceneActorNode : UNode
    {
        UInt32 mSceneId = UInt32.MaxValue;
        public override UInt32 SceneId
        {
            get => mSceneId;
            internal set => mSceneId = value;
        }
        public override bool IsSceneManagedType()
        {
            return true;
        }
    }
    public partial class ULightWeightNodeBase : UNode
    {
        public override bool IsSceneManagedType()
        {
            return false;
        }
        public override async System.Threading.Tasks.Task<bool> InitializeNode(GamePlay.UWorld world, UNodeData data, EBoundVolumeType bvType, Type placementType)            
        {
            return await base.InitializeNode(world, data, bvType, placementType);
        }
        public override UNode Parent
        {
            set
            {
                if (mParent == value)
                    return;
                var newScene = value.GetNearestParentScene();
                if (ParentScene != null && newScene != ParentScene)
                {
                    Profiler.Log.WriteLine(Profiler.ELogTag.Warning, "UNode", $"{GetType().FullName}:{NodeName} cann't move to another UScene");
                    return;
                }
                base.Parent = value;
            } 
        }
    }
}
