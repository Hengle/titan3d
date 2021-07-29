﻿using System;
using System.Collections.Generic;
using System.Text;

namespace EngineNS
{
    public unsafe partial struct IGpuBufferDesc
    {
        public void SetMode(bool isCpuWritable, bool isGpuWritable)
        {
            if ((!isCpuWritable) && (!isGpuWritable))
            {
                CPUAccessFlags = 0;
                BindFlags = (UInt32)(EBindFlag.BIND_SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_IMMUTABLE;
            }
            else if (isCpuWritable && (!isGpuWritable))
            {
                CPUAccessFlags = (UInt32)ECpuAccess.CAS_WRITE;
                BindFlags = (UInt32)(EBindFlag.BIND_SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_DYNAMIC;
            }
            else if ((!isCpuWritable) && isGpuWritable)
            {
                CPUAccessFlags = 0;
                BindFlags = (UInt32)(EBindFlag.BIND_UNORDERED_ACCESS | EBindFlag.BIND_SHADER_RESOURCE);
                Usage = EGpuUsage.USAGE_DEFAULT;
            }
            else
            {
                System.Diagnostics.Debug.Assert((!(isCpuWritable && isGpuWritable)));
            }
            MiscFlags = (UInt32)EResourceMiscFlag.BUFFER_STRUCTURED;
        }
    }
}

namespace EngineNS.RHI
{
    public class CGpuBuffer : AuxPtrType<IGpuBuffer>
    {
    }
}
