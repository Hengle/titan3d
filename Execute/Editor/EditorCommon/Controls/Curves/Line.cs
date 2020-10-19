using System.Windows;
using System.Windows.Media;

namespace EditorCommon.Controls.Curves
{
    /// <summary>
    /// ����֮�����ͷ��ֱ��
    /// </summary>
    public class Line : CurveBase
    {
        #region Fields
        /// <summary>
        /// �߶�
        /// </summary>
        private readonly LineSegment lineSegment = new LineSegment();

        #endregion Fields

        #region Protected Methods

        /// <summary>
        /// ���Figure
        /// </summary>
        /// <returns>PathSegment����</returns>
        protected override PathSegmentCollection FillFigure()
        {
            this.lineSegment.Point = this.EndPoint;
            return new PathSegmentCollection
            {
                this.lineSegment
            };
        }

        /// <summary>
        /// ��ȡ��ʼ��ͷ���Ľ�����
        /// </summary>
        /// <returns>��ʼ��ͷ���Ľ�����</returns>
        protected override Point GetStartArrowEndPoint()
        {
            return this.EndPoint;
        }

        /// <summary>
        /// ��ȡ������ͷ���Ŀ�ʼ��
        /// </summary>
        /// <returns>������ͷ���Ŀ�ʼ��</returns>
        protected override Point GetEndArrowStartPoint()
        {
            return this.StartPoint;
        }

        /// <summary>
        /// ��ȡ������ͷ���Ľ�����
        /// </summary>
        /// <returns>������ͷ���Ľ�����</returns>
        protected override Point GetEndArrowEndPoint()
        {
            return this.EndPoint;
        }

        #endregion  Protected Methods
    }
}
