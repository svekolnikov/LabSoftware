using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ControlCard.TH2830SCPI
{
    #region 7.1 Subsystem commands for TH2830
    enum CMD_TH2830
    {
        MEASlay,
        FREQuency,
        VOLTage,
        CURRent
    }
    #endregion
    #region 7.1.1 DISPlay subsystem commands
    enum MEASlay
    {
        PAGE,
        LINE,
        RFONt
    }
    enum PAGE
    {
        MEASurement,
        BNUMber,
        BCOunt,
        LIST,
        MSETup,
        CSETup,
        LTABle,
        LSETup,
        SYSTem,
        FLISt
    }
    enum RFONt
    {
        LARGe,
        TINY,
        OFF
    }
    #endregion
    #region 7.1.2 FREQuency subsystem commands
    enum FREQuency
    {
        VALUE,
        MIN,
        MAX
    }
    #endregion
    #region 7.1.3 VOLTage subsystem commands
    enum VOLTage
    {
        VALUE,
        MIN,
        MAX
    }
    #endregion
    #region 7.1.4 CURRent subsystem commands
    enum CURRent
    {
        VALUE,
        MIN,
        MAX
    }
    #endregion
    #region 7.1.6 Output RESister subsystem commands
    enum ORESister
    {
        R30,
        R100
    }
    #endregion

    class TH2830SCPI
    {
        private CMD_TH2830 _curCMD;

        public TH2830SCPI()
        {
            
        }

        private void SendCommand(CMD_TH2830 cmd)
        {
            
        }
    }
}
