using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpPt1
{
    public class CRs
    {
        List<RoutesRoute> routes;
        public List<CompoundRoutesCompoundRoute> compoundRoutes;
        List<CompoundRoutesCompoundRoute> existingCrs;
        List<CrStartEnd> crStartEnds;
        List<CompoundRoutesCompoundRouteRouteIDsRouteID> tmpCRts;
        public bool error;
        public CRs(List<RoutesRoute> routes, List<CrStartEnd> crStartEnds, List<CompoundRoutesCompoundRoute> compounds)
        {
            this.routes = routes;
            this.crStartEnds = crStartEnds;
            this.existingCrs = compounds;
        }

        public List<CompoundRoutesCompoundRoute> GetCompoundRoutes()
        {
            compoundRoutes = new List<CompoundRoutesCompoundRoute>();
            foreach (var cr in crStartEnds)
            {
                tmpCRts = new List<CompoundRoutesCompoundRouteRouteIDsRouteID>();
                this.error = !GetCrRoutesList(cr.Start, cr.End);
            }
            return compoundRoutes;
        }

        private bool GetCrRoutesList(string crStart, string crEnd)
        {
            Stack<Stack<RoutesRoute>> crStack = new Stack<Stack<RoutesRoute>>();
            string crStartInter = crStart;
            List<CompoundRoutesCompoundRoute> test = new List<CompoundRoutesCompoundRoute>();
            bool error = false;
            do
            {
                List<RoutesRoute> branches = routes
                                             .Where(x => x.Start == crStartInter &&
                                                         x.KindOfRoute != KindOfRouteType.shunting &&
                                                         x.Default == YesNoType.yes &&
                                                         x.Status.status != StatusType.deleted)
                                             //.DistinctBy(x => x.Start + x.Destination)
                                             .ToList();

                if (crStack.Count > 0 && (crStack.Count == Constants.maxCmpRoutesIteraion || branches.Count == 0))
                {
                    while (crStack.Peek().Count == 1)
                    {
                        crStack.Pop();
                        if (crStack.Count == 0)
                        {
                            error = true;
                            break;
                        }
                    }
                    if (error)
                    {
                        break;
                    }
                    crStack.Peek().Pop();
                    crStartInter = crStack.Peek().Peek().Destination;
                }
                else
                {
                    crStack.Push(new Stack<RoutesRoute>(branches));
                    try
                    {
                        crStartInter = crStack.Peek().Peek().Destination;
                    }
                    catch (InvalidOperationException e)
                    {
                        ErrLogger.Log(e.Message + crStart + "_" + crEnd);
                        error = true;
                        break;
                    }
                }
                //if (crStack.Peek().Peek().Destination == crEnd)
                //{
                //    test.Add(StackToCr(crStack, crStart, crEnd));
                //}
            }
            while (crStack.Count > 0 && crStack.Peek().Peek().Destination != crEnd);

            if (error)
            {
                ErrLogger.Log("Routes path between '" + crStart + "' and '" + crEnd + "' not found.");
                return !error;
            }

            CompoundRoutesCompoundRouteRouteIDsRouteID[] routeIDsRouteID =
                new CompoundRoutesCompoundRouteRouteIDsRouteID[crStack.Count];
            int j = 0;
            while (crStack.Count != 0)
            {
                routeIDsRouteID[j] = new CompoundRoutesCompoundRouteRouteIDsRouteID
                {
                    Value = crStack.Pop().Pop().Designation
                };
                j++;
            }
            Array.Reverse(routeIDsRouteID);
            if (routeIDsRouteID.Length > Constants.maxRoutesInCmRoute)
            {
                ErrLogger.Log("Routes count " + routeIDsRouteID.Length + " in '" + crStart + "_" + crEnd + "' from " + routeIDsRouteID[8].Value);
                return false;
            }
            CompoundRoutesCompoundRoute compoundRoute = new CompoundRoutesCompoundRoute
            {
                Designation = crStart + "_" + crEnd,
                Start = crStart,
                Destination = crEnd,
                RouteIDs = new CompoundRoutesCompoundRouteRouteIDs
                {
                    RouteID = routeIDsRouteID
                }
            };
            int crLength = compoundRoute.RouteIDs.RouteID.Length;
            string start = routes
                           .Where(x => x.Designation == compoundRoute.RouteIDs.RouteID[0].Value)
                           .FirstOrDefault().Start; //compoundRoute.RouteIDs.RouteID[0].Value.Split('_').First();
            for (int r = 2; r <= crLength; r++)
            {
                string end = routes
                           .Where(x => x.Designation == compoundRoute.RouteIDs.RouteID[r - 1].Value)
                           .FirstOrDefault().Destination;
                if (compoundRoutes.Any(x => x.Designation == start + "_" + end))
                {
                    continue;
                }
                CompoundRoutesCompoundRouteRouteIDsRouteID[] routeIDs =
                    new CompoundRoutesCompoundRouteRouteIDsRouteID[r];
                for (int rId = 0; rId < r; rId++)
                {
                    routeIDs[rId] = new CompoundRoutesCompoundRouteRouteIDsRouteID
                    {
                        Value = compoundRoute.RouteIDs.RouteID[rId].Value
                    };
                }
                CompoundRoutesCompoundRoute tmpcompoundRoute = new CompoundRoutesCompoundRoute
                {
                    Designation = start + "_" + end,
                    Status = new TStatus { status = StatusType.@new },
                    Start = start,
                    Destination = end,
                    RouteIDs = new CompoundRoutesCompoundRouteRouteIDs
                    {
                        RouteID = routeIDs
                    }
                };
                //CheckForDefaults(tmpcompoundRoute);
                if (this.routes.Any(x => x.Designation == tmpcompoundRoute.Designation))
                {
                    Logger.Log("Route exists with CR designation '" + tmpcompoundRoute.Designation + "'");
                    int counter = 1;
                    do
                    {
                        counter++;
                    } while (this.routes.Any(x => x.Designation == tmpcompoundRoute.Designation + "_" + counter));
                    tmpcompoundRoute.Designation = tmpcompoundRoute.Designation + "_" + counter;
                }
                if (!existingCrs.Any(x => x.Designation == tmpcompoundRoute.Designation))
                {
                    compoundRoutes.Add(tmpcompoundRoute);
                }
            }
            return !error;
            //compoundRoutes.Add(compoundRoute);
        }

        private void CheckForDefaults(CompoundRoutesCompoundRoute compoundRoute)
        {
            foreach (var rt in compoundRoute.RouteIDs.RouteID)
            {
                var nonDefRoutes = routes
                               .Where(x => x.Start == rt.Value.Split('_')[0] &&
                                         x.Destination == rt.Value.Split('_')[1] &&
                                         x.KindOfRoute != KindOfRouteType.shunting &&
                                         x.Default == YesNoType.no &&
                                         x.Status.status != StatusType.deleted)
                                         .ToList();
                if (nonDefRoutes.Count > 0)
                {
                    StringBuilder builder = new StringBuilder();
                    string delimiter = "";
                    foreach (var crtid in nonDefRoutes)
                    {
                        builder.Append(delimiter);
                        builder.Append(crtid.Designation);
                        delimiter = ", ";
                    }
                    ErrLogger.Log("CR '" + compoundRoute.Designation +
                                  "' has additional non-default routes: " + builder.ToString());
                }
            }
        }
    }

    public class CrStartEnd
    {
        public string Start { get; set; }
        public string End { get; set; }
    }

    public class CrStack
    {
        public RoutesRoute Routes { get; set; }
        public bool HasBranches { get; set; }
    }


}
