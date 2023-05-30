// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Microsoft.Acoustics.Editor
{
    // Simulation Config types
    [StructLayout(LayoutKind.Sequential)]
    public struct TritonProbeSamplingSpecification
    {
        public float MinHorizontalSpacing;
        public float MaxHorizontalSpacing;
        public float VerticalSpacing;
        public float MinHeightAboveGround;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TritonBoundingBox
    {
        public AcousticsPALPublic.TritonVec3d MinCorner;
        public AcousticsPALPublic.TritonVec3d MaxCorner;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TritonSimulationParameters
    {
        public float MeshUnitAdjustment;
        public float SceneScale;
        public float SpeedOfSound;
        public float SimulationFrequency;
        public float ReceiverSampleSpacing;
        public TritonProbeSamplingSpecification ProbeSpacing;
        public TritonBoundingBox PerProbeSimulationRegion;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TritonOperationalParameters
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string Prefix;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string WorkingDir;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string MaterialFilename;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string MeshFilename;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
        public string JobFilename;
        [MarshalAs(UnmanagedType.I1)]
        public bool DisablePml;
    }

    public class SimulationConfig : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected override bool ReleaseHandle()
        {
            SimulationConfigNativeMethods.TritonPreprocessor_SimulationConfiguration_Destroy(handle);
            return true;
        }

        SimulationConfig() : base(true) { }
    }

    public class SimulationConfigNativeMethods
    {
#if UNITY_EDITOR_WIN
        const string TritonPreprocessorDllName = "Triton.Preprocessor.dll";
#elif UNITY_EDITOR_OSX
        const string TritonPreprocessorDllName = "Triton.Preprocessor";
#else
#error Unsupported platform
#endif

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_Create(
            AcousticMesh mesh,
            ref TritonSimulationParameters simulationParams,
            ref TritonOperationalParameters opParams,
            AcousticMaterialLibrary materialLibrary,
            [MarshalAs(UnmanagedType.FunctionPtr)] Func<string, int, bool> callback,
            out SimulationConfig instance);

        // currently not called from managed tools
        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_CreateFromFile(
            [MarshalAs(UnmanagedType.LPStr)] string workingDir,
            [MarshalAs(UnmanagedType.LPStr)] string filename,
            out SimulationConfig instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern void TritonPreprocessor_SimulationConfiguration_Destroy(IntPtr instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_GetProbeCount(SimulationConfig instance, out int count);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_GetProbePoint(
            SimulationConfig instance,
            int probeIndex,
            ref AcousticsPALPublic.TritonVec3d location);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_GetVoxelMapInfo(
            SimulationConfig instance,
            ref TritonBoundingBox box,
            ref AcousticsPALPublic.TritonVec3i voxelCounts,
            out float cellSize);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_SimulationConfiguration_IsVoxelOccupied(
            SimulationConfig instance,
            AcousticsPALPublic.TritonVec3i location,
            [MarshalAs(UnmanagedType.I1)] out bool isOccupied);
    }


    // Acoustic Mesh Types
    enum MeshType
    {
        MeshTypeGeometry = 0,
        MeshTypeNavigation = 1,
        MeshTypeIncludeVolume = 2,
        MeshTypeExcludeVolume = 3,
        MeshTypeInvalid
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct TritonAcousticMeshTriangleInformation
    {
        public AcousticsPALPublic.TritonVec3i Indices;
        public long MaterialCode;
    }

    /// <summary>
    /// A safe handle implementation for AcousticMeshes. Prevents accidental memory leaks
    /// </summary>
    public class AcousticMesh : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected override bool ReleaseHandle()
        {
            AcousticMeshNativeMethods.TritonPreprocessor_AcousticMesh_Destroy(handle);
            return true;
        }

        public AcousticMesh() : base(true)
        {
        }
    }

    internal class AcousticMeshNativeMethods
    {
#if UNITY_EDITOR_WIN
        const string TritonPreprocessorDllName = "Triton.Preprocessor.dll";
#elif UNITY_EDITOR_OSX
        const string TritonPreprocessorDllName = "Triton.Preprocessor";
#else
#error Unsupported platform
#endif

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_AcousticMesh_Create(out AcousticMesh instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_AcousticMesh_CreateFromMeshFile(
            string meshFileName,
            string acousticMaterialFileName,
            float unitAdjustment,
            float sceneScale,
            out IntPtr meshInstance,
            out IntPtr matLibInstance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern void TritonPreprocessor_AcousticMesh_Destroy(IntPtr instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_AcousticMesh_Add(
            AcousticMesh instance,
            AcousticsPALPublic.TritonVec3d[] vertices,
            int vertexCount,
            TritonAcousticMeshTriangleInformation[] trianglesInfo,
            int trianglesCount,
            MeshType type
            );
    }

    // Acoustic Material Types
    /// <summary>
    /// A safe handle implementation for the material library. Prevents accidental memory leaks
    /// </summary>
    public class AcousticMaterialLibrary : SafeHandleZeroOrMinusOneIsInvalid
    {
        protected override bool ReleaseHandle()
        {
            AcousticMaterialNativeMethods.TritonPreprocessor_MaterialLibrary_Destroy(handle);
            return true;
        }

        public AcousticMaterialLibrary() : base(true)
        {
        }
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct TritonAcousticMaterial
    {
        // SizeConst = TRITON_MAX_NAME_LENGTH (in TritonPreprocessorApiTypes.h)
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string MaterialName;
        public float Absorptivity;
    }

    internal class AcousticMaterialNativeMethods
    {
#if UNITY_EDITOR_WIN
        const string TritonPreprocessorDllName = "Triton.Preprocessor.dll";
#elif UNITY_EDITOR_OSX
        const string TritonPreprocessorDllName = "Triton.Preprocessor";
#else
#error Unsupported platform
#endif
        // These are copied directly from the internal Triton definitions
        // Make sure to keep these in sync!!
        // Intentionally making struct, not enum, to allow storing longs instead of ints
        public struct MaterialCodes
        {
            /// <summary>   Reserved Code 0. </summary>
            public const long Reserved0 = long.MaxValue;
            /// <summary>   Reserved Code 1. </summary>
            public const long Reserved1 = long.MaxValue - 1;
            /// <summary>   Reserved Code 2. </summary>
            public const long Reserved2 = long.MaxValue - 2;
            /// <summary>   Reserved Code 3. </summary>
            public const long Reserved3 = long.MaxValue - 3;
            /// <summary>   Reserved Code 4. </summary>
            public const long Reserved4 = long.MaxValue - 4;
            /// <summary>   Reserved Code 5. </summary>
            public const long Reserved5 = long.MaxValue - 5;
            /// <summary>   Reserved Code 6. </summary>
            public const long Reserved6 = long.MaxValue - 6;
            /// <summary>   Reserved Code 7. </summary>
            public const long Reserved7 = long.MaxValue - 7;
            /// <summary>   Reserved Code 8. </summary>
            public const long Reserved8 = long.MaxValue - 8;
            /// <summary>   Reserved Code 9. </summary>
            public const long Reserved9 = long.MaxValue - 9;
            public const long TritonVolume = long.MaxValue - 10;
            public const long TritonNavigableArea = long.MaxValue - 11;
            /// <summary>   Global code for Air. </summary>
            public const long AirCode = 0;
            /// <summary>   Global code indicated open parts of a scene where sound is radiated and never returns. Eg. door, window etc. </summary>
            public const long OutsideCode = 1;
            public const float OutsideAbsorption = 1.0f;
            /// <summary>   Global default code for indicating wall. This is assumed whenever no code is provided for a voxel. </summary>
            public const long DefaultWallCode = 2;
            public const float DefaultWallAbsorption = 0.1f;
        }

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_CreateFromFile(
            [MarshalAs(UnmanagedType.LPStr)] string file,
            out AcousticMaterialLibrary instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_CreateFromMaterials(
            IntPtr materials,
            int count,
            out AcousticMaterialLibrary instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern void TritonPreprocessor_MaterialLibrary_Destroy(IntPtr instance);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_GetCount(AcousticMaterialLibrary instance, out int count);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_GetKnownMaterials(
            AcousticMaterialLibrary instance,
            IntPtr materials,
            IntPtr codes,
            int count);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_GetMaterialInfo(
            AcousticMaterialLibrary instance,
            long code,
            ref TritonAcousticMaterial material);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_GetMaterialCode(
            AcousticMaterialLibrary instance,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            out long code);

        [DllImport(TritonPreprocessorDllName)]
        public static extern bool TritonPreprocessor_MaterialLibrary_GuessMaterialCodeFromGeneralName(
            AcousticMaterialLibrary instance,
            [MarshalAs(UnmanagedType.LPStr)] string name,
            out long code);
    }
}