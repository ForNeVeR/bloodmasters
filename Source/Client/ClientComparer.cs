/********************************************************************\
*                                                                   *
*  Bloodmasters engine by Pascal vd Heiden, www.codeimp.com         *
*  All code in this file is my own design. You are free to use it.  *
*                                                                   *
\********************************************************************/

using System;
using System.Collections.Generic;

namespace Bloodmasters.Client;

public class ClientComparer : IComparer<Client>
{
    // Comparer method
    public int Compare(Client a, Client b)
    {
        int teama, teamb;
        int scorea, scoreb;
        int fragsa, fragsb;

        // Make team/spectator index
        teama = Scoreboard.GetSectionIndex(a);
        teamb = Scoreboard.GetSectionIndex(b);

        // Make reversed values
        scorea = 9999 - a.Score;
        scoreb = 9999 - b.Score;
        fragsa = 9999 - a.Frags;
        fragsb = 9999 - b.Frags;

        // Make comparable strings
        string sa = teama + "_" + scorea.ToString("0000000") + "_" + fragsa.ToString("0000") + "_" + a.Deaths.ToString("0000") + "_" + a.Name + "_" + a.ID.ToString("00");
        string sb = teamb + "_" + scoreb.ToString("0000000") + "_" + fragsb.ToString("0000") + "_" + b.Deaths.ToString("0000") + "_" + b.Name + "_" + b.ID.ToString("00");

        // Compare and return result
        return string.Compare(sa, sb, StringComparison.InvariantCulture);
    }
}
