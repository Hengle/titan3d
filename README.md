# �������л���
- 1.��װ2022
- 2.��װC#��������
- 3.���鰲װC#�ƶ���������
# ���빹��
## Windows��������
1. ����Core.Window���̣�C++��
2. ����Engine.Window���̣�C#��
3. ����MainEditor���̣�C#��
## Windows����Android APK
1. ����Core.Android���̣�C++��
2. ����Engine.Android�̣�C#��
# ����������
1. ����MainEditorΪ��ǰ��Ŀ
2. ���������в���Ϊconfig=\$(SolutionDir)content\EngineConfig.cfg use_renderdoc=false
3. ���Թ���Ŀ¼Ϊ$(SolutionDir)binaries\
4. Run/Debug
# ������ע������
1. ��Ҫ�ύ���ļ�(20M����)������lfsʹ��
2. [���ô���](:/3404bc5266aa4459a6e2be96b56ac5bf)
3. ��������C++��Bricksһ��Ҫ�ǵ����Ӷ�Ӧ�꣬�����C#�Ҳ���C++��������������ע������2
# ����̨����
## �������
- 1.ExeCmd=����ִ�е�����
- 2.ExtraCmd={n}���n��ȷ�������󣬿���̨��������Ĳ�������
## �����ʲ�������
- ����ָ���ʲ������°汾�����MetaVersion��ը����
ExeCmd=SaveAsLastest AssetType=Scene+Mesh+Material+MaterialInst+Texture CookCfg=\$(SolutionDir)content\EngineConfigForCook.cfg 
## ����Root������
- ����1��ExeCmd=StartRootServer CookCfg=\$(SolutionDir)content\EngineConfigForRootServer.cfg 
- ����2��ExtraCmd=1 CookCfg=$(SolutionDir)content\EngineConfigForRootServer.cfg �ڿ���̨����ExeCmd=StartRootServer
## ����Login������
- ����1��ExeCmd=StartLoginServer CookCfg=\$(SolutionDir)content\EngineConfigForRootServer.cfg 
- ����2��ExtraCmd=1 CookCfg=$(SolutionDir)content\EngineConfigForRootServer.cfg �ڿ���̨����ExeCmd=StartLoginServer