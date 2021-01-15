using Autodesk.AutoCAD.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Refact
{
    public class LongOperationManager : IDisposable //, System.Windows.Forms.IMessageFilter

    {
        // The message code corresponding to a keypress
        //const int WM_KEYDOWN = 0x0100;


        // The number of times to update the progress meter
        // (for some reason you need 600 to tick through
        //  for each percent)
        const int progressMeterIncrements = 600;

        // Internal members for metering progress
        private ProgressMeter pm;
        private long updateIncrement;
        private long currentInc;

        // External flag for checking cancelled status
        public bool cancelled = false;

        // Constructor
        public LongOperationManager(string message)

        {
            //System.Windows.Forms.Application.
            //  AddMessageFilter(this);

            pm = new ProgressMeter();
            pm.Start(message);
            pm.SetLimit(progressMeterIncrements);
            currentInc = 0;
        }

        // System.IDisposable.Dispose

        public void Dispose()
        {
            pm.Stop();
            pm.Dispose();
            //System.Windows.Forms.Application.
            //  RemoveMessageFilter(this);
        }


        // Set the total number of operations
        public void SetTotalOperations(long totalOps)
        {
            // We really just care about when we need
            // to update the timer
            updateIncrement =
              (totalOps > progressMeterIncrements ?
                totalOps / progressMeterIncrements :
                totalOps
              );
        }

        // This function is called whenever an operation
        // is performed

        public bool Tick(int increment)
        {
            if (++currentInc == updateIncrement)
            {
                for (int i = 0; i < increment; i++)
                {
                    pm.MeterProgress();
                    currentInc = 0;
                    System.Windows.Forms.Application.DoEvents();
                }
               
            }

            // Check whether the filter has set the flag
            if (cancelled)
                pm.Stop();
            return !cancelled;
        }


        // The message filter callback
        //public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        //{
        //    if (m.Msg == WM_KEYDOWN)
        //    {
        //        // Check for the Escape keypress
        //        System.Windows.Forms.Keys kc =
        //          (System.Windows.Forms.Keys)(int)m.WParam &
        //          System.Windows.Forms.Keys.KeyCode;

        //        if (m.Msg == WM_KEYDOWN &&
        //            kc == System.Windows.Forms.Keys.Escape)
        //        {
        //            cancelled = true;
        //        }

        //        // Return true to filter all keypresses
        //        return true;
        //    }

        //    // Return false to let other messages through
        //    return false;
        //}

    }
}
