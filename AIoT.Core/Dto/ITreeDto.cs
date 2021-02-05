using System;
using System.Collections.Generic;

namespace AIoT.Core.Dto
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="Tkey"></typeparam>
    public interface ITreeDto<Tkey>
    {
        /// <summary>
        /// 
        /// </summary>
        Tkey Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        Tkey ParentId { get; set; }

      
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        void AddChildren(ITreeDto<Tkey> node);
    }


    /// <summary>
    /// 
    /// </summary>
    public static class TreeExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        public static List<TDto> ToTree<TDto> (this List<TDto> source) where TDto : ITreeDto<string>
        {
            var dtoMap = new Dictionary<string, TDto>();
            foreach (var item in source)
            {
                dtoMap.Add(item.Id, item);
            }
            List<TDto> result = new List<TDto>();
            foreach (var item in dtoMap.Values)
            {
                //如果父节点不存在，那么本身就是父节点
                if (dtoMap.ContainsKey(item.ParentId ?? "0"))
                {
                    dtoMap[item.ParentId ?? "0"].AddChildren(item);
                }
                else
                {
                    result.Add(item);
                }

            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="TDto"></typeparam>
        /// <returns></returns>
        public static List<TDto> ToTreeLong<TDto>(this List<TDto> source) where TDto : ITreeDto<long>
        {
            var dtoMap = new Dictionary<long, TDto>();
            foreach (var item in source)
            {
                dtoMap.Add(item.Id, item);
            }
            List<TDto> result = new List<TDto>();
            foreach (var item in dtoMap.Values)
            {
                //如果父节点不存在，那么本身就是父节点
                if (dtoMap.ContainsKey(item.ParentId))
                {
                    dtoMap[item.ParentId].AddChildren(item);
                }
                else
                {
                    result.Add(item);
                }

            }
            return result;
        }
    }
}
