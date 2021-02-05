using System.ComponentModel.DataAnnotations;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// ��ѯ�ȽϷ�ʽ
    /// </summary>
    public enum QueryCompare
    {
        /// <summary>
        /// ����
        /// </summary>
        [Display(Name = "����")]
        Equal,

        /// <summary>
        /// ������
        /// </summary>
        [Display(Name = "������")]
        NotEqual,

        /// <summary>
        /// ģ��ƥ��
        /// </summary>
        [Display(Name = "ģ��ƥ��")]
        Like,

        /// <summary>
        /// ������ģ��ƥ��
        /// </summary>
        [Display(Name = "������ģ��ƥ��")]
        NotLike,

        /// <summary>
        /// ��...��ͷ
        /// </summary>
        [Display(Name = "��...��ͷ")]
        StartWidth,

        /// <summary>
        /// ��...��β
        /// </summary>
        [Display(Name = "��...��β")]
        EndsWith,

        /// <summary>
        /// С��
        /// </summary>
        [Display(Name = "С��")]
        LessThan,

        /// <summary>
        /// С�ڵ���
        /// </summary>
        [Display(Name = "С�ڵ���")]
        LessThanOrEqual,

        /// <summary>
        /// ����
        /// </summary>
        [Display(Name = "����")]
        GreaterThan,

        /// <summary>
        /// ���ڵ���
        /// </summary>
        [Display(Name = "���ڵ���")]
        GreaterThanOrEqual,

        /// <summary>
        /// ��...֮��(���ڵ�����ʼ��С�ڽ���)�����Ա�����һ�����ϣ��򶺺ŷָ����ַ�������ȡ��һ�����һ��ֵ��
        /// ���Ա���������򼯺����ͣ� ���磺
        /// [Query(QueryCompare.Between)]
        /// public DateTime[] CreationDate { get; set; }
        /// </summary>
        [Display(Name = "��...֮��")]
        Between,

        /// <summary>
        /// ���ڵ�����ʼ��С�ڽ��������Ա�����һ�����ϣ��򶺺ŷָ����ַ�������ȡ��һ�����һ��ֵ��
        /// ���Ա���������򼯺����ͣ� ���磺
        /// [Query(QueryCompare.GreaterEqualAndLess)]
        /// public DateTime[] CreationDate { get; set; }
        /// </summary>
        [Display(Name = "���ڵ�����ʼ��С�ڽ���")]
        GreaterEqualAndLess,

        /// <summary>
        /// ���ڵ�����ʼ��С�ڵ��ڽ��������Ա�����һ�����ϣ��򶺺ŷָ����ַ�������ȡ��һ�����һ��ֵ��
        /// ���Ա���������򼯺����ͣ� ���磺
        /// [Query(QueryCompare.GreaterEqualAndLessEqual)]
        /// public DateTime[] CreationDate { get; set; }
        /// </summary>
        [Display(Name = "���ڵ�����ʼ��С�ڵ��ڽ���")]
        GreaterEqualAndLessEqual,

        /// <summary>
        /// ���������Ա�����һ�����ϣ��򶺺ŷָ����ַ�����
        /// ���Ա���������򼯺����ͣ� ���磺
        /// [Query(QueryCompare.Include)]
        /// public string[] Ids { get; set; }
        /// </summary>
        [Display(Name = "����")]
        Include,

        /// <summary>
        /// �����������Ա�����һ�����ϣ��򶺺ŷָ����ַ�����
        /// </summary>
        [Display(Name = "������")]
        NotInclude,

        /// <summary>
        /// Ϊ�ջ�Ϊ�գ�����Ϊ bool���ͣ���ɿ����͡�
        /// </summary>
        [Display(Name = "Ϊ�ջ�Ϊ��")]
        IsNull,

        /// <summary>
        /// �Ƿ����ָ��ö��
        /// </summary>
        [Display(Name = "�Ƿ����ָ��ö��")]
        HasFlag,
    }
}