using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VolPro.Core.Log
{
    public class ActionChangeLog(ActionChangeChannel changeChannel)
    {
        private readonly ActionChangeChannel _changeChannel = changeChannel;

        public void Write<T>(ActionChangeType changeType, T originalData, T changedData=null, string requestParameter = null) where T : class
        {
            _changeChannel.Write(changeType, originalData, changedData,requestParameter);
        }
        public async Task WriteAsync<T>(ActionChangeType changeType, T originalData, T changedData=null, string requestParameter = null) where T : class
        {
            await _changeChannel.WriteAsync(changeType, originalData, changedData,requestParameter);
        }
        public void WriteRange<T>(ActionChangeType changeType, List<T> originalData, string requestParameter = null) where T : class
        {
            _changeChannel.WriteRange(changeType, originalData,requestParameter);
        }
    }
}
