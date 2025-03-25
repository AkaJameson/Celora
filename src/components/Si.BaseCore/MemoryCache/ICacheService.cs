namespace Si.CoreHub.MemoryCache
{
    public interface ICacheService
    {
        /// <summary>
        /// 获取缓存
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TValue> Get<TKey, TValue>(TKey key);
        /// <summary>
        /// 设置缓存*绝对过期
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="AbsoluteExpiration"></param>
        /// <returns></returns>
        Task<TValue> SetAbsolute<TKey, TValue>(TKey key, TValue value, TimeSpan? AbsoluteExpiration = null);
        /// <summary>
        /// 设置缓存*滑动过期
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="SlidingExpiration"></param>
        /// <returns></returns>
        Task<TValue> SetSliding<TKey, TValue>(TKey key, TValue value, TimeSpan? SlidingExpiration = null);
        /// <summary>
        /// 移除缓存
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task Remove<TKey>(TKey key);
        /// <summary>
        /// 判断缓存是否存在
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<bool> Exists<TKey>(TKey key);
        /// <summary>
        /// 获取缓存并移除
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        Task<TValue> GetAndRemove<TKey, TValue>(TKey key);

    }
}