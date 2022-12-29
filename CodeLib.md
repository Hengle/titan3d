# ���ô���
## 1.���ܷ���
```C#
[ThreadStatic]
private static Profiler.TimeScope ScopeTick = Profiler.TimeScopeManager.GetTimeScope(typeof(UMovement), nameof(TickLogic));
using (new Profiler.TimeScopeHelper(ScopeTick))
{
	//do sth
}
```
## 2.�������
1.ȷ�����dll��������ʵ��
UPluginLoader���������������
AssemblyEntry����������DLLģ����UTypeDesc����Ϣ�ģ�ͨ���������ͺ�Meta��Ϣ
```C#
namespace EngineNS.Plugins.��Ĳ����
{
    public class UPluginLoader
    {
        public static UGameItemPlugin? mPluginObject = new UGameItemPlugin();
        public static Bricks.AssemblyLoader.UPlugin GetPluginObject()
        {
            return mPluginObject;
        }
    }
}

namespace EngineNS.Rtti
{
    public class AssemblyEntry
    {
        public class UGameServerAssemblyDesc : UAssemblyDesc
        {
            public UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:��Ĳ���� AssemblyDesc Created");
            }
            ~UGameServerAssemblyDesc()
            {
                Profiler.Log.WriteLine(Profiler.ELogTag.Info, "Core", "Plugins:��Ĳ���� AssemblyDesc Destroyed");
            }
            public override string Name { get => "��Ĳ����"; }
            public override string Service { get { return "Plugins"; } }
            public override bool IsGameModule { get { return false; } }
            public override string Platform { get { return "Global"; } }
        }
        static UGameServerAssemblyDesc AssmblyDesc = new UGameServerAssemblyDesc();
        public static UAssemblyDesc GetAssemblyDesc()
        {
            return AssmblyDesc;
        }
    }
}

```
2.���������·��
binaries\Plugins\Debug\net6.0
3.�ڲ��Ŀ¼��Ҫ��Ӳ��ͬ��.plugin�ļ������ݴ������£�
```XML
<?xml version="1.0" encoding="utf-8"?>
<Root Type="EngineNS.Bricks.AssemblyLoader.UPluginDescriptor@EngineCore">
  <LoadOnInit Type="System.Boolean@Unknown" Value="False" />
  <Platforms Type="System.Collections.Generic.List&lt;EngineNS.EPlatformType@EngineCore,&gt;@Unknown" Count="1">
    <e_0 Value="PLTF_Windows" />
  </Platforms>
  <Dependencies Type="System.Collections.Generic.List&lt;System.String@Unknown,&gt;@Unknown" Count="1">
    <e_0 Value="GameServer" />
  </Dependencies>
</Root>
```
PluginsĿ¼��CopyPlugins.bat���޸�*.plugin��Ŀǰ��Ҫ�ֹ�ִ�У�ˢ�µ����Ŀ¼
## 3.Native Bricks����
Ŀǰ��Ҫ��Base/BaseHead.h�������ƽ̨������� #define HasModule_NextRHI 
�������ȷ�����ɽ�ˮ���������빹��������ᷢ��C#����C++�Ҳ�������