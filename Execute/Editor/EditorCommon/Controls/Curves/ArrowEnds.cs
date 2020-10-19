using System;

namespace EditorCommon.Controls.Curves
{
    [Flags]
    public enum ArrowHalf
    {
        /// <summary>
        /// �����ͷ
        /// </summary>
        Up = 0,

        /// <summary>
        /// �Ҳ���ͷ
        /// </summary>
        Down= 1,

        /// <summary>
        /// �����ͷ
        /// </summary>
        Both = 2,
    }
    /// <summary>
    /// ��ͷ���ڶ�
    /// </summary>
    [Flags]
    public enum ArrowEnds
    {
        /// <summary>
        /// �޼�ͷ
        /// </summary>
        None = 0,

        /// <summary>
        /// ��ʼ�����ͷ
        /// </summary>
        Start = 1,

        /// <summary>
        /// ���������ͷ
        /// </summary>
        End = 2,

        /// <summary>
        /// ���˼�ͷ
        /// </summary>
        Both = 3
    }
}