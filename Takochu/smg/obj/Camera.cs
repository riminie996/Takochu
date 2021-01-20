﻿using GL_EditorFramework;
using GL_EditorFramework.EditorDrawables;
using GL_EditorFramework.GL_Core;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Takochu.fmt;
using Takochu.ui;
using Takochu.util;

namespace Takochu.smg.obj
{
    public class Camera : SingleObject
    {
        public enum CameraType
        {
            Cube,
            Event,
            Group,
            Start,
            Other
        }

        public Dictionary<string, CameraType> cameraDict = new Dictionary<string, CameraType>()
        {
            { "c", CameraType.Cube },
            { "e", CameraType.Event },
            { "g", CameraType.Group },
            { "s", CameraType.Start },
            { "o", CameraType.Other }
        };

        public Camera(BCSV.Entry entry, Zone parent) : base(Vector3.Zero)
        {
            mEntry = entry;
            mName = mEntry.Get<string>("id");
            mParentZone = parent;

            mType = mEntry.Get<string>("camtype");

            mFields = new Dictionary<string, object>();

            // the first thing we load are the camera-type specific fields
            List<string> fields = CameraUtil.GetStrings(mType).ToList();

            foreach(string field in fields)
            {
                string type = CameraUtil.GetTypeOfField(field);

                switch (type)
                {
                    case "float":
                        mFields[field] = mEntry.Get<float>(field);
                        break;
                    case "uint":
                        mFields[field] = mEntry.Get<uint>(field);
                        break;
                    case "string":
                        mFields[field] = mEntry.Get<string>(field);
                        break;
                }
            }

            Dictionary<string, string> allFields = CameraUtil.GetAll();

            foreach(string s in allFields.Keys)
            {
                string type = CameraUtil.GetTypeOfField(s);

                if (!mEntry.ContainsKey(s))
                    continue;

                switch (type)
                {
                    case "float":
                        mFields[s] = mEntry.Get<float>(s);
                        break;
                    case "uint":
                        mFields[s] = mEntry.Get<uint>(s);
                        break;
                    case "string":
                        mFields[s] = mEntry.Get<string>(s);
                        break;
                }
            }
        }

        public CameraType GetCameraType()
        {
            string type = mName.Substring(0, 1);
            return cameraDict.ContainsKey(type) ? cameraDict[type] : CameraType.Cube;
        }

        public override string ToString()
        {
            return $"{mName} [{mParentZone.mZoneName}]";
        }

        public override uint Select(int index, GL_ControlBase control)
        {
            if (!Selected)
            {
                Selected = true;
            }
            return 0;
        }

        public override uint SelectDefault(GL_ControlBase control)
        {
            if (!Selected)
            {
                Selected = true;
            }
            return 0;
        }

        public override uint SelectAll(GL_ControlBase control)
        {
            if (!Selected)
            {
                Selected = true;
            }
            return 0;
        }

        public override uint Deselect(int index, GL_ControlBase control)
        {
            if (Selected)
            {
                Selected = false;
            }
            return 0;
        }

        public override uint DeselectAll(GL_ControlBase control)
        {
            if (Selected)
            {
                Selected = false;
            }
            return 0;
        }

        public override bool TrySetupObjectUIControl(EditorSceneBase scene, ObjectUIControl objectUIControl)
        {
            if (!Selected)
                return false;

            objectUIControl.AddObjectUIContainer(new CameraUI(this, scene), "General Settings");

            if (mFields.Keys.Any(k => k.StartsWith("flag.")))
                objectUIControl.AddObjectUIContainer(new CameraFlagsUI(this, scene), "Camera Flags");

            // camera types "Cube", "Group", "Start", and "Other" load the chunk CameraParamChunkGame
            // "Event" loads CameraParamChunkEvent
            CameraType type = GetCameraType();

            if (type == CameraType.Cube || type == CameraType.Group || type == CameraType.Start || type == CameraType.Other)
                objectUIControl.AddObjectUIContainer(new CameraGameFlagsUI(this, scene), "Camera Game Flags");

            if (type == CameraType.Event)
                objectUIControl.AddObjectUIContainer(new CameraEventFlagsUI(this, scene), "Camera Event Flags");

            return true;
        }

        public class CameraUI : IObjectUIContainer
        {
            Camera obj;
            EditorSceneBase scene;

            static readonly string[] cameraTypes = new string[]
            {
                "CAM_TYPE_XZ_PARA",
                "CAM_TYPE_TOWER",
                "CAM_TYPE_FOLLOW",
                "CAM_TYPE_WONDER_PLANET",
                "CAM_TYPE_POINT_FIX",
                "CAM_TYPE_EYEPOS_FIX",
                "CAM_TYPE_SLIDER",
                "CAM_TYPE_INWARD_TOWER",
                "CAM_TYPE_EYEPOS_FIX_THERE",
                "CAM_TYPE_TRIPOD_BOSS",
                "CAM_TYPE_TOWER_POS",
                "CAM_TYPE_TRIPOD_PLANET",
                "CAM_TYPE_DEAD",
                "CAM_TYPE_INWARD_SPHERE",
                "CAM_TYPE_RAIL_DEMO",
                "CAM_TYPE_RAIL_FOLLOW",
                "CAM_TYPE_TRIPOD_BOSS_JOINT",
                "CAM_TYPE_CHARMED_TRIPOD_BOSS",
                "CAM_TYPE_OBJ_PARALLEL",
                "CAM_TYPE_CHARMED_FIX",
                "CAM_TYPE_GROUND",
                "CAM_TYPE_TRUNDLE",
                "CAM_TYPE_CUBE_PLANET",
                "CAM_TYPE_INNER_CYLINDER",
                "CAM_TYPE_SPIRAL_DEMO",
                "CAM_TYPE_TALK",
                "CAM_TYPE_MTXREG_PARALLEL",
                "CAM_TYPE_CHARMED_VECREG",
                "CAM_TYPE_MEDIAN_PLANET",
                "CAM_TYPE_TWISTED_PASSAGE",
                "CAM_TYPE_MEDIAN_TOWER",
                "CAM_TYPE_CHARMED_VECREG_TOWER",
                "CAM_TYPE_FRONT_AND_BACK",
                "CAM_TYPE_RACE_FOLLOW",
                "CAM_TYPE_2D_SLIDE",
                "CAM_TYPE_FOO_FIGHTER",
                "CAM_TYPE_FOO_FIGHTER_PLANET",
                "CAM_TYPE_BLACK_HOLE",
                "CAM_TYPE_ANIM",
                "CAM_TYPE_DPD",
                "CAM_TYPE_WATER_FOLLOW",
                "CAM_TYPE_WATER_PLANET",
                "CAM_TYPE_WATER_PLANET_BOSS",
                "CAM_TYPE_RAIL_WATCH",
                "CAM_TYPE_SUBJECTIVE",
                "CAM_TYPE_SPHERE_TRUNDLE",
                "CAM_TYPE_FREEZE"
            };

            public CameraUI(Camera obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                obj.mName = control.FullWidthTextInput(obj.mName, "Camera Name");
                obj.mType = control.DropDownTextInput("Camera Type", obj.mEntry.Get<string>("camtype"), cameraTypes, false);

                obj.mFields["camint"] = Convert.ToUInt32(control.NumberInput((uint)obj.mFields["camint"], "CamInt"));
            }

            public void OnValueChangeStart()
            {

            }

            public void OnValueChanged()
            {
                scene.Refresh();
            }

            public void OnValueSet()
            {

            }

            public void UpdateProperties()
            {

            }
        }

        public class CameraFlagsUI : IObjectUIContainer
        {
            Camera obj;
            EditorSceneBase scene;

            public CameraFlagsUI(Camera obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                if (obj.mFields.ContainsKey("flag.noreset"))
                    obj.mFields["flag.noreset"] = Convert.ToUInt32(control.CheckBox("No Reset", (uint)obj.mFields["flag.noreset"] != 0));

                if (obj.mFields.ContainsKey("flag.nofovy"))
                    obj.mFields["flag.nofovy"] = Convert.ToUInt32(control.CheckBox("No Fovy", (uint)obj.mFields["flag.nofovy"] != 0));

                if (obj.mFields.ContainsKey("flag.lofserpoff"))
                    obj.mFields["flag.lofserpoff"] = Convert.ToUInt32(control.CheckBox("lofserpoff", (uint)obj.mFields["flag.lofserpoff"] != 0));

                if (obj.mFields.ContainsKey("flag.antibluroff"))
                    obj.mFields["flag.antibluroff"] = Convert.ToUInt32(control.CheckBox("No Anti-Blur", (uint)obj.mFields["flag.antibluroff"] != 0));

                if (obj.mFields.ContainsKey("flag.collisionoff"))
                    obj.mFields["flag.collisionoff"] = Convert.ToUInt32(control.CheckBox("No Collision", (uint)obj.mFields["flag.collisionoff"] != 0));

                if (obj.mFields.ContainsKey("flag.subjectiveoff"))
                    obj.mFields["flag.subjectiveoff"] = Convert.ToUInt32(control.CheckBox("Subjective Off", (uint)obj.mFields["flag.subjectiveoff"] != 0));
            }

            public void OnValueChangeStart()
            {

            }

            public void OnValueChanged()
            {
                scene.Refresh();
            }

            public void OnValueSet()
            {

            }

            public void UpdateProperties()
            {

            }
        }

        public class CameraGameFlagsUI : IObjectUIContainer
        {
            Camera obj;
            EditorSceneBase scene;

            public CameraGameFlagsUI(Camera obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                if (obj.mFields.ContainsKey("gflag.thru"))
                    obj.mFields["gflag.thru"] = Convert.ToUInt32(control.CheckBox("gflag.thru", (uint)obj.mFields["gflag.thru"] != 0));

                if (obj.mFields.ContainsKey("gflag.enableEndErpFrame"))
                    obj.mFields["gflag.enableEndErpFrame"] = Convert.ToUInt32(control.CheckBox("gflag.enableEndErpFrame", (uint)obj.mFields["gflag.enableEndErpFrame"] != 0));

                if (obj.mFields.ContainsKey("gflag.camendint"))
                    obj.mFields["gflag.camendint"] = Convert.ToUInt32(control.CheckBox("gflag.camendint", (uint)obj.mFields["gflag.camendint"] != 0));
            }

            public void OnValueChangeStart()
            {

            }

            public void OnValueChanged()
            {
                scene.Refresh();
            }

            public void OnValueSet()
            {

            }

            public void UpdateProperties()
            {

            }
        }

        public class CameraEventFlagsUI : IObjectUIContainer
        {
            Camera obj;
            EditorSceneBase scene;

            public CameraEventFlagsUI(Camera obj, EditorSceneBase scene)
            {
                this.obj = obj;
                this.scene = scene;
            }

            public void DoUI(IObjectUIControl control)
            {
                if (obj.mFields.ContainsKey("eflag.enableErpFrame"))
                    obj.mFields["eflag.enableErpFrame"] = Convert.ToUInt32(control.CheckBox("eflag.enableErpFrame", (uint)obj.mFields["eflag.enableErpFrame"] != 0));

                if (obj.mFields.ContainsKey("eflag.enableEndErpFrame"))
                    obj.mFields["eflag.enableEndErpFrame"] = Convert.ToUInt32(control.CheckBox("eflag.enableEndErpFrame", (uint)obj.mFields["eflag.enableEndErpFrame"] != 0));

                if (obj.mFields.ContainsKey("camendint"))
                    obj.mFields["camendint"] = Convert.ToUInt32(control.NumberInput((uint)obj.mFields["camendint"], "camendint"));

                if (obj.mFields.ContainsKey("evfrm"))
                    obj.mFields["evfrm"] = Convert.ToUInt32(control.NumberInput((uint)obj.mFields["evfrm"], "evfrm"));

                if (obj.mFields.ContainsKey("evpriority"))
                    obj.mFields["evpriority"] = Convert.ToUInt32(control.NumberInput((uint)obj.mFields["evpriority"], "evpriority"));
            }

            public void OnValueChangeStart()
            {

            }

            public void OnValueChanged()
            {
                scene.Refresh();
            }

            public void OnValueSet()
            {

            }

            public void UpdateProperties()
            {

            }
        }

        public BCSV.Entry mEntry;
        public string mName;
        public Zone mParentZone;

        public string mType;
        /*public uint mVersion;
        public Vector3 mWorldOffset;
        public float mLookOffset;
        public float mVerticalLookOffset;
        public float mRoll;
        public float mFovy;
        public int mCamInt;
        public float mUpper;
        public float mLower;
        public int mGNDInt;
        public float mUPlay;
        public float mLPlay;
        public int mPushDelay;
        public int mPushDelayLow;
        public int mUDown;
        public int mPanUse;
        public Vector3 mPanAxis;

        public uint mNoReset;
        public uint mNoFovy;
        public uint mLOFserpOff;
        public uint mAntiBlurOff;
        public uint mCollisionOff;
        public uint mSubjectiveOff;

        public float mDist;
        public Vector3 mAxis;
        public Vector3 mWorldPoint;
        public Vector3 mUp;

        public float angleA;
        public float angleB;
        public uint mNum1;
        public uint mNum2;

        public string mString;*/

        Dictionary<string, object> mFields;
    }
}