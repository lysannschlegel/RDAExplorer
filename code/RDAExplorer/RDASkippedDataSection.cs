using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RDAExplorer
{
    public class RDASkippedDataSection
    {
        public BlockInfo blockInfo;
        public ulong offset;
        public ulong size; // size of the encrypted data only. the block info must be added if needed.
    }
}
