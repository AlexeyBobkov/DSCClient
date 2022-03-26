//#define DBG_TIME

//
// Most of the formulas used for calculations were borrowed from the Paul Schlyter, Stockholm, Sweden web site pages:
// http://www.stjarnhimlen.se/comp/ppcomp.html
// http://www.stjarnhimlen.se/comp/tutorial.html
//
using System;

namespace SkyObjectPosition
{
    public struct SkyObjectPosCalc
    {
        // FUNCTIONS

        public static double CalcTime(int iYear, int iMonth, int iDay, int iHour, int iMin, int iSec, int imSec)
        {
#if DBG_TIME
            iYear = 2008;
            iMonth = 6;
            iDay = 16;
            iHour = 19;
            iMin = 0;
            iSec = 0;
            imSec = 0;
#endif
            double d = 367 * iYear - (7 * (iYear + ((iMonth + 9) / 12))) / 4 + (275 * iMonth) / 9 + iDay - 730530;
            d += (iHour + (iMin + (iSec + imSec / 1000.0) / 60.0) / 60.0) / 24.0;
            return d;
        }

        // local siderial time in degree
        public static double LST(double d, double longitude)
        {
            // The formula was borrowed from Wikipedia:
            // http://en.wikipedia.org/wiki/Sidereal_time
            // To an accuracy within 0.1 second per century, Greenwich (Mean) Sidereal Time (in hours and decimal parts of an hour) can be calculated as
            // GMST = 18.697 374 558 + 24.065 709 824 419 08 * D ,
            // where D is the interval, in UT1 days including any fraction of a day, since 2000 January 1,
            // at 12h UT (interval counted positive if forwards to a later time than the 2000 reference instant),
            // and the result is freed from any integer multiples of 24 hours to reduce it
            // to a value in the range 0–24
            //
            //double dGSMT = 18.697374558 + 24.06570982441908 * (d-1.5);
            //return Rev(dGSMT * 15 + longitude);
            return Rev(360.9856473662862 * d + 98.9821473205707 + longitude);

            // alternative formula from the Paul Schlyter: use Sun mean longtitude
            //double Ls = sunOrbit_.M(d, false) + sunOrbit_.w(d, false);
            //return Rev(Ls + 180.0 + d * 360.0 + longitude);
        }

        public static double Rev(double a)
        {
            return a - Math.Floor(a / 360.0) * 360.0;
        }

        public static double ecl(double d) { return 23.4393 - 3.563E-7 * d; }

        static public void Equ2AzAlt(double d, double lat, double lon, double dec, double ra, out double az, out double alt)
        {
            double dHA = LST(d, lon) - ra;                  // hour angle
            double x = CosD(dHA) * CosD(dec);
            double y = SinD(dHA) * CosD(dec);
            double z = SinD(dec);

            az = Atan2D(y, x * SinD(lat) - z * CosD(lat)) + 180;
            alt = AsinD(x * CosD(lat) + z * SinD(lat));

            // Atmospheric refraction, Sæmundsson formula for P = 1010, T = 10 (Jean Meeus, "Astronomical Algorithms")
            if (alt > -2 && alt < 89)
                alt += 0.017 / TanD(alt + 10.3 / (alt + 5.11));
        }

        static public void AzAlt2Equ(double d, double lat, double lon, double az, double alt, out double dec, out double ra)
        {
            // Atmospheric refraction, Bennett formula (Jean Meeus, "Astronomical Algorithms")
            if (alt > -2 && alt < 89)
                alt -= 1 / (60 * TanD(alt + 7.31 / (alt + 4.4)));

            az -= 180;
            double y = SinD(az) * CosD(alt);
            double x = CosD(az) * CosD(alt);
            double z = SinD(alt);

            double dHA = Atan2D(y, x * SinD(lat) + z * CosD(lat));  // hour angle
            ra = LST(d, lon) - dHA;
            dec = AsinD(-x * CosD(lat) + z * SinD(lat));
        }

        public static double SinD(double a) { return Math.Sin(a * Math.PI / 180.0); }
        public static double CosD(double a) { return Math.Cos(a * Math.PI / 180.0); }
        public static double TanD(double a) { return Math.Tan(a * Math.PI / 180.0); }

        public static double AsinD(double x) { return Math.Asin(x) * (180.0 / Math.PI); }
        public static double AtanD(double x) { return Math.Atan(x) * (180.0 / Math.PI); }
        public static double Atan2D(double y, double x) { return Math.Atan2(y, x) * (180.0 / Math.PI); }

        // linear expression
        public struct LinExpr
        {
            private double x0_, x1_;
            public LinExpr(double x0, double x1)
            {
                x0_ = x0;
                x1_ = x1;
            }
            public double Val(double d) { return x0_ + x1_ * d; }
        }

        // Standard Kepler orbit elements.
        // (Distance units may be different.)
        public struct Orbit
        {
            private LinExpr N_;
            private LinExpr i_;
            private LinExpr w_;
            private LinExpr a_;
            private LinExpr e_;
            private LinExpr M_;

            //longitude of the ascending node
            public double N(double d) { return N(d, true); }
            public double N(double d, bool rev)
            {
                double v = N_.Val(d);
                return rev ? Rev(v) : v;
            }

            //inclination to the ecliptic (plane of the Earth's orbit)
            public double i(double d) { return i_.Val(d); }

            //argument of perihelion
            public double w(double d) { return w(d, true); }
            public double w(double d, bool rev)
            {
                double v = w_.Val(d);
                return rev ? Rev(v) : v;
            }

            //semi-major axis, or mean distance from Sun
            public double a(double d) { return a_.Val(d); }

            //eccentricity (0=circle, 0-1=ellipse, 1=parabola)
            public double e(double d) { return e_.Val(d); }

            //mean anomaly (0 at perihelion; increases uniformly with time)
            public double M(double d) { return M(d, true); }
            public double M(double d, bool rev)
            {
                double v = M_.Val(d);
                return rev ? Rev(v) : v;
            }

            //longitude of perihelion
            public double w1(double d) { return N(d) + w(d); }

            //mean longitude
            public double L(double d) { return M(d) + w1(d); }

            //perihelion distance
            public double q(double d) { return a(d) * (1 - e(d)); }

            //aphelion distance
            public double Q(double d) { return a(d) * (1 + e(d)); }

            //eccentric anomaly
            public double E(double d) { return E(d, true, 0.001); }
            public double E(double d, bool rev, double precision)
            {
                double dM = M(d);
                double de = e(d);
                double dE = dM + de * (180 / Math.PI) * SinD(dM) * (1.0 + de * CosD(dM));
                if (de >= 0.05)
                {
                    for (int i = 20; --i >= 0; )
                    {
                        double dE0 = dE;
                        double dE0rev = Rev(dE0);
                        dE = dE0 - (dE0 - de * (180 / Math.PI) * SinD(dE0rev) - dM) / (1 - de * CosD(dE0rev));
                        if (Math.Abs(dE - dE0) < precision)
                            break;
                    }
                }
                return rev ? Rev(dE) : dE;
            }

            public Orbit(double N0, double N1,
                            double i0, double i1,
                            double w0, double w1,
                            double a0, double a1,
                            double e0, double e1,
                            double M0, double M1)
            {
                N_ = new LinExpr(N0, N1);
                i_ = new LinExpr(i0, i1);
                w_ = new LinExpr(w0, w1);
                a_ = new LinExpr(a0, a1);
                e_ = new LinExpr(e0, e1);
                M_ = new LinExpr(M0, M1);
            }
        }

        // distance unit is Earth radius for Moon; all coordinates are geocentric
        static Orbit moonOrbit_ = new Orbit(125.1228, -0.0529538083, 5.1454, 0, 318.0634, 0.1643573223, 60.2666, 0, 0.054900, 0, 115.3654, 13.0649929509);

        // distance unit is Astronomical Unit for all these orbits; all coordinates are heliocentric
        static Orbit sunOrbit_ = new Orbit(0, 0, 0, 0, 282.9404, 4.70935E-5, 1, 0, 0.016709, -1.151E-9, 356.0470, 0.9856002585),
                        mercuryOrbit_ = new Orbit(48.3313, 3.24587E-5, 7.0047, 5.00E-8, 29.1241, 1.01444E-5, 0.387098, 0, 0.205635, 5.59E-10, 168.6562, 4.0923344368),
                        venusOrbit_ = new Orbit(76.6799, 2.46590E-5, 3.3946, 2.75E-8, 54.8910, 1.38374E-5, 0.723330, 0, 0.006773, -1.302E-9, 48.0052, 1.6021302244),
                        marsOrbit_ = new Orbit(49.5574, 2.11081E-5, 1.8497, -1.78E-8, 286.5016, 2.92961E-5, 1.523688, 0, 0.093405, 2.516E-9, 18.6021, 0.5240207766),
                        jupiterOrbit_ = new Orbit(100.4542, 2.76854E-5, 1.3030, -1.557E-7, 273.8777, 1.64505E-5, 5.20256, 0, 0.048498, 4.469E-9, 19.8950, 0.0830853001),
                        saturnOrbit_ = new Orbit(113.6634, 2.38980E-5, 2.4886, -1.081E-7, 339.3939, 2.97661E-5, 9.55475, 0, 0.055546, -9.499E-9, 316.9670, 0.0334442282),
                        uranusOrbit_ = new Orbit(74.0005, 1.3978E-5, 0.7733, 1.9E-8, 96.6612, 3.0565E-5, 19.18171, -1.55E-8, 0.047318, 7.45E-9, 142.5905, 0.011725806),
                        neptuneOrbit_ = new Orbit(131.7806, 3.0173E-5, 1.7700, -2.55E-7, 272.8461, -6.027E-6, 30.05826, 3.313E-8, 0.008606, 2.15E-9, 260.2471, 0.005995147);

        // abstract class to calculate position
        public abstract class SkyPosition
        {
            public abstract string Name { get; }
            public virtual string Info { get { return ""; } }
            public virtual string NameInfo { get { return (Info != null && Info.Length > 0) ? Name + " - " + Info : Name; } }

            public abstract void CalcEquatorial(double d, out double rg, out double dec, out double ra);

            protected virtual double CalcTopoParallax(double rg) { return -1; }

            public void CalcTopoRaDec(double d, double lat, double lon, out double dec, out double ra)
            {
                double rg;
                CalcEquatorial(d, out rg, out dec, out ra);

                double mpar = CalcTopoParallax(rg);                 // parallax
                if (mpar > 0)
                {
                    double gclat = lat - 0.1924 * SinD(2 * lat);    // geocentric latitude
                    double rho = 0.99833 + 0.00167 * CosD(2 * lat); // distance from the center of the Earth
                    double dHA = LST(d, lon) - ra;                  // hour angle
                    double g = AtanD(TanD(gclat) / CosD(dHA));      // auxiliary angle

                    // calculate topocentroc RA and Decl
                    ra -= mpar * rho * CosD(gclat) * SinD(dHA) / CosD(dec);
                    if (g < -0.001 || g > 0.001)
                        dec -= mpar * rho * SinD(gclat) * SinD(g - dec) / SinD(g);
                    else
                        dec -= mpar * rho * SinD(-dec) * CosD(dHA);
                }
            }

            public void CalcAzimuthal(double d, double lat, double lon, out double az, out double alt)
            {
                double rg, dec, ra;
                CalcEquatorial(d, out rg, out dec, out ra);

                Equ2AzAlt(d, lat, lon, dec, ra, out az, out alt);

                double mpar = CalcTopoParallax(rg);             // parallax
                if (mpar > 0)
                    alt -= mpar * CosD(alt);
            }
        };

        // abstract class to calculate position in equatorial coordinates
        public abstract class GenSkyPos : SkyPosition
        {
            private Orbit orbit_;

            // calc distance and true longitude
            public void CalcDistLong(double d, out double r, out double lon)
            {
                double de = orbit_.e(d);
                double dE = orbit_.E(d);
                double da = orbit_.a(d);
                double xv = da * (CosD(dE) - de);
                double yv = da * Math.Sqrt(1.0 - de * de) * SinD(dE);

                double v = Atan2D(yv, xv);          // true anomaly
                r = Math.Sqrt(xv * xv + yv * yv);   // distance
                lon = Rev(v + orbit_.w(d));         // true longitude
            }

            protected Orbit orbit
            {
                get { return orbit_; }
            }

            public GenSkyPos(Orbit orbit)
            {
                orbit_ = orbit;
            }
        };

        // Sun position
        public class SunSkyPos : GenSkyPos
        {
            public SunSkyPos() : base(sunOrbit_) { }

            public override string Name { get { return "Sun"; } }
            public override void CalcEquatorial(double d, out double rg, out double dec, out double ra)
            {
                // distance r, true anomaly v, true longitude lon
                double lon;
                CalcDistLong(d, out rg, out lon);

                // Convert lonsun,r to ecliptic rectangular geocentric coordinates xs,ys
                double xs = rg * CosD(lon);
                double ys = rg * SinD(lon);

                // Convert this to equatorial, rectangular, geocentric coordinates
                double decl = ecl(d);
                double xe = xs;
                double ye = ys * CosD(decl);
                double ze = ys * SinD(decl);

                // Finally, compute the Sun's Right Ascension (RA) and Declination (Dec)
                ra = Rev(Atan2D(ye, xe));
                dec = Atan2D(ze, Math.Sqrt(xe * xe + ye * ye));
            }
        }
        public static SunSkyPos sunPos_ = new SunSkyPos();

        // Generic planet position
        public class PlanetSkyPos : GenSkyPos
        {
            private string name_;
            protected virtual bool hasPerturbations
            {
                get { return false; }
            }

            protected virtual void CalcPerturbations(double d, ref double lonecl, ref double latecl, ref double r) { }

            protected virtual void ConvertToGeocentric(double d, double xh, double yh, double zh, out double xg, out double yg, out double zg)
            {
                // calculate Sun position
                double lonsun, rs;
                sunPos_.CalcDistLong(d, out rs, out lonsun);
                xg = xh + rs * CosD(lonsun);
                yg = yh + rs * SinD(lonsun);
                zg = zh;
            }

            public PlanetSkyPos(string name, Orbit orbit) : base(orbit) { name_ = name; }

            public override string Name { get { return name_; } }
            public override void CalcEquatorial(double d, out double rg, out double dec, out double ra)
            {
                // distance r, true anomaly v, true longitude lon
                double r, lon;
                CalcDistLong(d, out r, out lon);

                // Convert lon,r to ecliptic rectangular heliocentric coordinates
                double dN = orbit.N(d);
                double di = orbit.i(d);
                double xh = r * (CosD(dN) * CosD(lon) - SinD(dN) * SinD(lon) * CosD(di));
                double yh = r * (SinD(dN) * CosD(lon) + CosD(dN) * SinD(lon) * CosD(di));
                double zh = r * SinD(lon) * SinD(di);

                if (hasPerturbations)
                {
                    // Convert to ecliptic longitude, latitude (distance == r)
                    double lonecl = Rev(Atan2D(yh, xh));
                    double latecl = Atan2D(zh, Math.Sqrt(xh * xh + yh * yh));

                    // Calculate perturbations
                    CalcPerturbations(d, ref lonecl, ref latecl, ref r);

                    // Convert back to ecliptic rectangular heliocentric coordinates
                    xh = r * CosD(lonecl) * CosD(latecl);
                    yh = r * SinD(lonecl) * CosD(latecl);
                    zh = r * SinD(latecl);
                }

                // Convert this to ecliptic, rectangular, geocentric coordinates
                double xg, yg, zg;
                ConvertToGeocentric(d, xh, yh, zh, out xg, out yg, out zg);

                // Convert this to equatorial, rectangular, geocentric coordinates
                double decl = ecl(d);
                double xe = xg;
                double ye = yg * CosD(decl) - zg * SinD(decl);
                double ze = yg * SinD(decl) + zg * CosD(decl);

                // Finally, compute the Sun's Right Ascension (RA) and Declination (Dec)
                ra = Rev(Atan2D(ye, xe));
                dec = Atan2D(ze, Math.Sqrt(xe * xe + ye * ye));
                rg = Math.Sqrt(xe * xe + ye * ye + ze * ze);
            }
        }
        // Position of planet close to earth
        public class NearPlanetSkyPos : PlanetSkyPos
        {
            protected override double CalcTopoParallax(double rg)
            {
                //return (8.794/3600) / rg;
                return 0.0024428 / rg;
            }

            public NearPlanetSkyPos(string name, Orbit orbit) : base(name, orbit) { }
        }

        // some planets without perturbations
        public static PlanetSkyPos mercuryPos_ = new NearPlanetSkyPos("Mercury", mercuryOrbit_);
        public static PlanetSkyPos venusPos_ = new NearPlanetSkyPos("Venus", venusOrbit_);
        public static PlanetSkyPos marsPos_ = new NearPlanetSkyPos("Mars", marsOrbit_);
        public static PlanetSkyPos neptunePos_ = new PlanetSkyPos("Neptune", neptuneOrbit_);

        // Moon position
        public class MoonSkyPos : PlanetSkyPos
        {
            public MoonSkyPos() : base("Moon", moonOrbit_) { }

            protected override bool hasPerturbations
            {
                get { return true; }
            }

            protected override void CalcPerturbations(double d, ref double lonecl, ref double latecl, ref double r)
            {
                // Calculate Moon perturbations
                double dN = orbit.N(d);
                double dMs = sunOrbit_.M(d, false);         // Sun mean anomaly
                double dMm = orbit.M(d, false);             // Moon mean anomaly
                double dLs = dMs + sunOrbit_.w(d, false);   // Sun mean longtitude
                double dLm = dN + dMm + orbit.w(d, false);  // Moon mean longtitude
                double dD = dLm - dLs;                      // mean elongation of the Moon
                double dF = dLm - dN;                       // argument of latitude for the Moon

                lonecl += -1.274 * SinD(Rev(dMm - 2 * dD))          // (the Evection)
                            + 0.658 * SinD(Rev(2 * dD))             // (the Variation)
                            - 0.186 * SinD(Rev(dMs))                // (the Yearly Equation)
                            - 0.059 * SinD(Rev(2 * dMm - 2 * dD))
                            - 0.057 * SinD(Rev(dMm - 2 * dD + dMs))
                            + 0.053 * SinD(Rev(dMm + 2 * dD))
                            + 0.046 * SinD(Rev(2 * dD - dMs))
                            + 0.041 * SinD(Rev(dMm - dMs))
                            - 0.035 * SinD(Rev(dD))                 // (the Parallactic Equation)
                            - 0.031 * SinD(Rev(dMm + dMs))
                            - 0.015 * SinD(Rev(2 * dF - 2 * dD))
                            + 0.011 * SinD(Rev(dMm - 4 * dD));
                latecl += -0.173 * SinD(Rev(dF - 2 * dD))
                            - 0.055 * SinD(Rev(dMm - dF - 2 * dD))
                            - 0.046 * SinD(Rev(dMm + dF - 2 * dD))
                            + 0.033 * SinD(Rev(dF + 2 * dD))
                            + 0.017 * SinD(Rev(2 * dMm + dF));
                r += -0.58 * CosD(Rev(dMm - 2 * dD))
                            - 0.46 * CosD(Rev(2 * dD));
            }

            protected override void ConvertToGeocentric(double d, double xh, double yh, double zh, out double xg, out double yg, out double zg)
            {
                // no conversion: we already in geocentric coordinates
                xg = xh;
                yg = yh;
                zg = zh;
            }

            protected override double CalcTopoParallax(double rg)
            {
                return AsinD(1 / rg);
            }
        }
        public static MoonSkyPos moonPos_ = new MoonSkyPos();

        // Jupiter position
        public class JupiterSkyPos : PlanetSkyPos
        {
            public JupiterSkyPos() : base("Jupiter", jupiterOrbit_) { }
            protected override bool hasPerturbations { get { return true; } }
            protected override void CalcPerturbations(double d, ref double lonecl, ref double latecl, ref double r)
            {
                double dMj = jupiterOrbit_.M(d, false);         // Jupiter mean anomaly
                double dMs = saturnOrbit_.M(d, false);          // Saturn mean anomaly
                lonecl += -0.332 * SinD(Rev(2 * dMj - 5 * dMs - 67.6))
                            - 0.056 * SinD(Rev(2 * dMj - 2 * dMs + 21))
                            + 0.042 * SinD(Rev(3 * dMj - 5 * dMs + 21))
                            - 0.036 * SinD(Rev(dMj - 2 * dMs))
                            + 0.022 * CosD(Rev(dMj - dMs))
                            + 0.023 * SinD(Rev(2 * dMj - 3 * dMs + 52))
                            - 0.016 * SinD(Rev(dMj - 5 * dMs - 69));
            }
        }
        public static JupiterSkyPos jupiterPos_ = new JupiterSkyPos();

        // Saturn position
        public class SaturnSkyPos : PlanetSkyPos
        {
            public SaturnSkyPos() : base("Saturn", saturnOrbit_) { }
            protected override bool hasPerturbations { get { return true; } }
            protected override void CalcPerturbations(double d, ref double lonecl, ref double latecl, ref double r)
            {
                double dMj = jupiterOrbit_.M(d, false);         // Jupiter mean anomaly
                double dMs = saturnOrbit_.M(d, false);          // Saturn mean anomaly
                lonecl += +0.812 * SinD(Rev(2 * dMj - 5 * dMs - 67.6))
                            - 0.229 * CosD(Rev(2 * dMj - 4 * dMs - 2))
                            + 0.119 * SinD(Rev(dMj - 2 * dMs - 3))
                            + 0.046 * SinD(Rev(2 * dMj - 6 * dMs - 69))
                            + 0.014 * SinD(Rev(dMj - 3 * dMs + 32));
                latecl += -0.020 * CosD(Rev(2 * dMj - 4 * dMs - 2))
                            + 0.018 * SinD(Rev(2 * dMj - 6 * dMs - 49));
            }
        }
        public static SaturnSkyPos saturnPos_ = new SaturnSkyPos();

        // Uranus position
        public class UranusSkyPos : PlanetSkyPos
        {
            public UranusSkyPos() : base("Uranus", uranusOrbit_) { }
            protected override bool hasPerturbations { get { return true; } }
            protected override void CalcPerturbations(double d, ref double lonecl, ref double latecl, ref double r)
            {
                double dMj = jupiterOrbit_.M(d, false);         // Jupiter mean anomaly
                double dMs = saturnOrbit_.M(d, false);          // Saturn mean anomaly
                double dMu = uranusOrbit_.M(d, false);          // Uranus mean anomaly
                lonecl += +0.040 * SinD(Rev(dMs - 2 * dMu + 6))
                            + 0.035 * SinD(Rev(dMs - 3 * dMu + 33))
                            - 0.015 * SinD(Rev(dMj - dMu + 20));
            }
        }
        public static UranusSkyPos uranusPos_ = new UranusSkyPos();

        // Pluto position: special model
        public class PlutoSkyPos : SkyPosition
        {
            public override string Name { get { return "Pluto"; } }
            public override void CalcEquatorial(double d, out double rg, out double dec, out double ra)
            {
                double S = 50.03 + 0.033459652 * d;
                double P = 238.95 + 0.003968789 * d;

                double lonecl = 238.9508 + 0.00400703 * d
                                - 19.799 * SinD(P) + 19.848 * CosD(P)
                                + 0.897 * SinD(2 * P) - 4.956 * CosD(2 * P)
                                + 0.610 * SinD(3 * P) + 1.211 * CosD(3 * P)
                                - 0.341 * SinD(4 * P) - 0.190 * CosD(4 * P)
                                + 0.128 * SinD(5 * P) - 0.034 * CosD(5 * P)
                                - 0.038 * SinD(6 * P) + 0.031 * CosD(6 * P)
                                + 0.020 * SinD(S - P) - 0.010 * CosD(S - P);
                double latecl = -3.9082
                                - 5.453 * SinD(P) - 14.975 * CosD(P)
                                + 3.527 * SinD(2 * P) + 1.673 * CosD(2 * P)
                                - 1.051 * SinD(3 * P) + 0.328 * CosD(3 * P)
                                + 0.179 * SinD(4 * P) - 0.292 * CosD(4 * P)
                                + 0.019 * SinD(5 * P) + 0.100 * CosD(5 * P)
                                - 0.031 * SinD(6 * P) - 0.026 * CosD(6 * P)
                                                      + 0.011 * CosD(S - P);
                double r = 40.72
                                + 6.68 * SinD(P) + 6.90 * CosD(P)
                                - 1.18 * SinD(2 * P) - 0.03 * CosD(2 * P)
                                + 0.15 * SinD(3 * P) - 0.14 * CosD(3 * P);

                // Convert this to ecliptic rectangular heliocentric coordinates
                double xh = r * CosD(lonecl) * CosD(latecl);
                double yh = r * SinD(lonecl) * CosD(latecl);
                double zh = r * SinD(latecl);

                // Convert this to ecliptic, rectangular, geocentric coordinates
                double lonsun, rs;
                sunPos_.CalcDistLong(d, out rs, out lonsun);
                double xg = xh + rs * CosD(lonsun);
                double yg = yh + rs * SinD(lonsun);
                double zg = zh;

                // Convert this to equatorial, rectangular, geocentric coordinates
                double decl = ecl(d);
                double xe = xg;
                double ye = yg * CosD(decl) - zg * SinD(decl);
                double ze = yg * SinD(decl) + zg * CosD(decl);

                // Finally, compute the Pluto's Right Ascension (RA) and Declination (Dec)
                ra = Rev(Atan2D(ye, xe));
                dec = Atan2D(ze, Math.Sqrt(xe * xe + ye * ye));
                rg = Math.Sqrt(xe * xe + ye * ye + ze * ze);
            }
        };
        public static PlutoSkyPos plutoPos_ = new PlutoSkyPos();

        public class StarPosition : SkyPosition
        {
            private string name_, info_;
            private double dec_, ra_;
            private bool isJ2000_;

            public StarPosition(string name, double ra_hour, double dec)
                : this(name, "", ra_hour, dec, true)
            {
            }
            public StarPosition(string name, double ra_hour, double dec, bool isJ2000)
                : this(name, "", ra_hour, dec, isJ2000)
            {
            }
            public StarPosition(string name, string info, double ra_hour, double dec)
                : this(name, info, ra_hour, dec, true)
            {
            }
            public StarPosition(string name, string info, double ra_hour, double dec, bool isJ2000)
            {
                name_ = name;
                info_ = (info == null) ? "" : info;
                dec_ = dec;
                ra_ = ra_hour * 15;
                isJ2000_ = isJ2000;
            }
            public override string Name { get { return name_; } }
            public override string Info { get { return info_; } }
            public override void CalcEquatorial(double d, out double rg, out double dec, out double ra)
            {
                rg = 1E15;
                dec = dec_;
                ra = ra_;

                if (isJ2000_)
                {
                    // Precession
                    // The formula for precession was borrowed from the site:
                    // http://star-www.st-and.ac.uk/~fv/webnotes/chapt16.htm
                    // The values for 2000 are approximately: 
                    // m = 3.075 seconds of time per year 
                    // n = 1.336 seconds of time per year
                    //     = 20.043 arc-seconds per year
                    //
                    // Correction to α and δ (i.e. ra and dec): 
                    // Δα = m + n sin(α) tan(δ) 
                    // Δδ = n cos(α)
                    double years = d / 365.2422;
                    double ra_corr = (0.0128125 + 0.0055675 * SinD(ra) * TanD(dec)) * years;
                    double dec_corr = (0.0055675 * CosD(ra)) * years;
                    ra += ra_corr;
                    dec += dec_corr;
                }
            }
        }

        public static SkyPosition[] sunSystemObjects =
        {
            SkyObjectPosCalc.sunPos_,
            SkyObjectPosCalc.moonPos_,
            SkyObjectPosCalc.mercuryPos_,
            SkyObjectPosCalc.venusPos_,
            SkyObjectPosCalc.marsPos_,
            SkyObjectPosCalc.jupiterPos_,
            SkyObjectPosCalc.saturnPos_,
            SkyObjectPosCalc.uranusPos_,
            SkyObjectPosCalc.neptunePos_,
            SkyObjectPosCalc.plutoPos_
        };

        public static SkyPosition[] stars =
        {
            new StarPosition("Achernar",1.62854213,-57.23666007),
            new StarPosition("Adhara",6.9770963,-28.97208931),
            new StarPosition("Alcyone",3.79140671,24.10524193),
            new StarPosition("Aldebaran",4.59866679,16.50976164),
            new StarPosition("Alderamin",21.30960598,62.58545529),
            new StarPosition("Algenib",0.22059721,15.18361593),
            new StarPosition("Algieba",10.3328227,19.84186032),
            new StarPosition("Algol",3.13614714,40.9556512),
            new StarPosition("Alhena",6.62852842,16.39941482),
            new StarPosition("Alioth",12.9004536,55.95984301),
            new StarPosition("Alkaid",13.79237392,49.31330288),
            new StarPosition("Alnair",22.13718789,-46.96061593),
            new StarPosition("Alnath",5.43819386,28.60787346),
            new StarPosition("Alnilam",5.60355904,-1.20191725),
            new StarPosition("Alnitak",5.67931244,-1.94257841),
            new StarPosition("Alphard",9.45979217,-8.65868335),
            new StarPosition("Alphekka",15.57810819,26.71491041),
            new StarPosition("Alpheratz",0.13976888,29.09082805),
            new StarPosition("Altair",19.84630057,8.86738491),
            new StarPosition("Ankaa",0.43801871,-42.30512197),
            new StarPosition("Antares",16.49012986,-26.43194608),
            new StarPosition("Arcturus",14.2612076,19.18726997),
            new StarPosition("Arneb",5.54550386,-17.82229227),
            new StarPosition("Bellatrix",5.41885227,6.34973451),
            new StarPosition("Betelgeuse",5.91952477,7.40703634),
            new StarPosition("Canopus",6.39919184,-52.69571799),
            new StarPosition("Capella",5.27813767,45.99902927),
            new StarPosition("Caph",0.15280269,59.15021814),
            new StarPosition("Castor",7.57666793,31.88863645),
            new StarPosition("Deneb",20.69053151,45.28033423),
            new StarPosition("Denebola",11.81774398,14.57233687),
            new StarPosition("Diphda",0.7264523,-17.9866841),
            new StarPosition("Dubhe",11.06217691,61.75111888),
            new StarPosition("Enif",21.73642787,9.87500791),
            new StarPosition("Etamin",17.94343829,51.48895101),
            new StarPosition("Fomalhaut",22.96078488,-29.62183701),
            new StarPosition("Hamal",2.11952383,23.46277743),
            new StarPosition("Izar",14.74979191,27.07417383),
            new StarPosition("Kaus Australis",18.40287397,-34.3843146),
            new StarPosition("Kochab",14.84510983,74.15547596),
            new StarPosition("Markab",23.07933801,15.20536786),
            new StarPosition("Menkar",3.03799418,4.08992539),
            new StarPosition("Merak",11.0306641,56.38234478),
            new StarPosition("Mirach",1.16216599,35.62083048),
            new StarPosition("Mirphak",3.40537459,49.86124281),
            new StarPosition("Mizar",13.39872773,54.92541525),
            new StarPosition("Nihal",5.47075731,-20.75923214),
            new StarPosition("Nunki",18.92108797,-26.29659428),
            new StarPosition("Phad",11.89715035,53.69473296),
            new StarPosition("Polaris",2.52974312,89.26413805),
            new StarPosition("Pollux",7.75537884,28.02631031),
            new StarPosition("Procyon",7.65514946,5.22750767),
            new StarPosition("Rasalgethi",17.24412845,14.39025314),
            new StarPosition("Rasalhague",17.58222354,12.56057584),
            new StarPosition("Regulus",10.13957205,11.96719513),
            new StarPosition("Rigel",5.24229756,-8.20163919),
            new StarPosition("Saiph",5.79594109,-9.66960186),
            new StarPosition("Scheat",23.06287038,28.08245462),
            new StarPosition("Shaula",17.56014624,-37.10374835),
            new StarPosition("Shedir",0.67510756,56.53740928),
            new StarPosition("Sirius",6.7525694,-16.71314306),
            new StarPosition("Spica",13.41989015,-11.16124491),
            new StarPosition("Tarazed",19.77099171,10.61326869),
            new StarPosition("Unukalhai",15.7377766,6.42551971),
            new StarPosition("Vega",18.61560722,38.78299311),
            new StarPosition("Vindemiatrix",13.03632237,10.95910186)
        };

        public static SkyPosition[] messier =
        {
            new StarPosition("M1", "Crab Nebula (PN)",5.575,22.01666667),
            new StarPosition("M2", "(GC)",21.55833333,-0.833333333),
            new StarPosition("M3", "(GC)",13.70333333,28.38333333),
            new StarPosition("M4", "(GC)",16.395,-26.52575),
            new StarPosition("M5", "(GC)",15.30833333,2.083333333),
            new StarPosition("M6", "Butterfly Cluster (OC)",17.66833333,-32.21666667),
            new StarPosition("M7", "Ptolemy's Cluster (OC)",17.89833333,-34.81666667),
            new StarPosition("M8", "Lagoon Nebula (DN)",18.05333333,-24.38333333),
            new StarPosition("M9", "(GC)",17.31833333,-18.51666667),
            new StarPosition("M10", "(GC)",16.95166667,-4.116666667),
            new StarPosition("M11", "Wild Duck Cluster (OC)",18.85166667,-6.266666667),
            new StarPosition("M12", "(GC)",16.78666667,-1.95),
            new StarPosition("M13", "The Hercules Cluster (GC)",16.695,36.45),
            new StarPosition("M14", "(GC)",17.62666667,-3.25),
            new StarPosition("M15", "(GC)",21.5,12.16666667),
            new StarPosition("M16", "Eagle Nebula (DN)",18.31333333,-13.78333333),
            new StarPosition("M17", "Omega Nebula (DN)",18.34666667,-16.18333333),
            new StarPosition("M18", "(OC)",18.33166667,-17.13333333),
            new StarPosition("M19", "(GC)",17.04333333,-26.25),
            new StarPosition("M20", "Trifid Nebula (DN)",18.03166667,-23.03333333),
            new StarPosition("M21", "(OC)",18.07833333,-22.5),
            new StarPosition("M22", "Sagittarius Cluster (GC)",18.60666667,-23.91666667),
            new StarPosition("M23", "(OC)",17.94833333,-19.01666667),
            new StarPosition("M24", "Sagittarius Star Cloud (SC)",18.30666667,-18.41666667),
            new StarPosition("M25", "(OC)",18.52833333,-19.25),
            new StarPosition("M26", "(OC)",18.755,-9.4),
            new StarPosition("M27", "Dumbbell Nebula (PN)",19.99333333,22.71666667),
            new StarPosition("M28", "(GC)",18.41,-24.86666667),
            new StarPosition("M29", "(OC)",20.39833333,38.53333333),
            new StarPosition("M30", "(GC)",21.67166667,-23.18333333),
            new StarPosition("M31", "Andromeda Galaxy (GX)",0.711666667,41.26666667),
            new StarPosition("M32", "Satellite Galaxy of M31 (GX)",0.711666667,40.86666667),
            new StarPosition("M33", "Triangulum Galaxy (GX)",1.563333333,30.65),
            new StarPosition("M34", "(OC)",2.7,42.78333333),
            new StarPosition("M35", "(OC)",6.148333333,24.33333333),
            new StarPosition("M36", "(OC)",5.588333333,34.15),
            new StarPosition("M37", "(OC)",5.873333333,32.55),
            new StarPosition("M38", "(OC)",5.478333333,35.83333333),
            new StarPosition("M39", "(OC)",21.53666667,48.43333333),
            new StarPosition("M40", "Winnecke 4 (DS)",12.39,58.05),
            new StarPosition("M41", "(OC)",6.783333333,-20.73333333),
            new StarPosition("M42", "The Orion Nebula (DN)",5.59,-5.383333333),
            new StarPosition("M43", "De Mairan's Nebula (DN)",5.593333333,-5.266666667),
            new StarPosition("M44", "Beehive Cluster (OC)",8.671666667,19.81666667),
            new StarPosition("M45", "Pleiades (OC)",3.785,24.11666667),
            new StarPosition("M46", "(OC)",7.696666667,-14.81666667),
            new StarPosition("M47", "(OC)",7.61,-14.48333333),
            new StarPosition("M48", "(OC)",8.228333333,-5.783333333),
            new StarPosition("M49", "(GX)",12.495,7.983333333),
            new StarPosition("M50", "(OC)",7.05,-8.333333333),
            new StarPosition("M51", "Whirlpool Galaxy (GX)",13.49833333,47.2),
            new StarPosition("M52", "(OC)",23.40333333,61.58333333),
            new StarPosition("M53", "(GC)",13.215,18.16666667),
            new StarPosition("M54", "(GC)",18.92,-30.46666667),
            new StarPosition("M55", "(GC)",19.66833333,-30.93333333),
            new StarPosition("M56", "(GC)",19.27666667,30.16666667),
            new StarPosition("M57", "Ring Nebula (PN)",18.89333333,33.03333333),
            new StarPosition("M58", "(GX)",12.625,11.81666667),
            new StarPosition("M59", "(GX)",12.7,11.65),
            new StarPosition("M60", "(GX)",12.72666667,11.55),
            new StarPosition("M61", "(GX)",12.36666667,4.466666667),
            new StarPosition("M62", "(GC)",17.02166667,-30.11666667),
            new StarPosition("M63", "Sunflower Galaxy (GX)",13.26333333,42.03333333),
            new StarPosition("M64", "Black Eye Galaxy (GX)",12.94666667,21.68333333),
            new StarPosition("M65", "Leo Triplet (GX)",11.315,13.1),
            new StarPosition("M66", "Leo Triplet (GX)",11.33666667,13),
            new StarPosition("M67", "(OC)",8.841666667,11.81666667),
            new StarPosition("M68", "(GC)",12.65833333,-26.75),
            new StarPosition("M69", "(GC)",18.52333333,-32.35),
            new StarPosition("M70", "(GC)",18.72166667,-32.3),
            new StarPosition("M71", "(GC)",19.895,18.78333333),
            new StarPosition("M72", "(GC)",20.89,-12.55),
            new StarPosition("M73", "(Asterism)",20.985,-12.63333333),
            new StarPosition("M74", "(GX)",1.611666667,15.78333333),
            new StarPosition("M75", "(GC)",20.10166667,-21.91666667),
            new StarPosition("M76", "Little Dumbbell Nebula (PN)",1.7,51.56666667),
            new StarPosition("M77", "(GX)",2.711666667,-0.016666667),
            new StarPosition("M78", "(DN)",5.78,0.05),
            new StarPosition("M79", "(GC)",5.405,-24.51666667),
            new StarPosition("M80", "(GC)",16.285,-22.98333333),
            new StarPosition("M81", "Bode's Galaxy (GX)",9.926666667,69.06666667),
            new StarPosition("M82", "Cigar Galaxy (GX)",9.933333333,69.7),
            new StarPosition("M83", "Southern Pinwheel Galaxy (GX)",13.61666667,-29.86666667),
            new StarPosition("M84", "(GX)",12.41666667,12.88333333),
            new StarPosition("M85", "(GX)",12.42333333,18.18333333),
            new StarPosition("M86", "(GX)",12.43666667,12.93333333),
            new StarPosition("M87", "Virgo A (GX)",12.51333333,12.38333333),
            new StarPosition("M88", "(GX)",12.53333333,14.41666667),
            new StarPosition("M89", "(GX)",12.59333333,12.55),
            new StarPosition("M90", "(GX)",12.61333333,13.16666667),
            new StarPosition("M91", "(GX)",12.59166667,14.5),
            new StarPosition("M92", "(GC)",17.285,43.15),
            new StarPosition("M93", "(OC)",7.741666667,-23.86666667),
            new StarPosition("M94", "(GX)",12.84833333,41.13333333),
            new StarPosition("M95", "(GX)",10.73166667,11.7),
            new StarPosition("M96", "(GX)",10.77833333,11.81666667),
            new StarPosition("M97", "Owl Nebula (PN)",11.24833333,55.03333333),
            new StarPosition("M98", "(GX)",12.23,14.9),
            new StarPosition("M99", "(GX)",12.31333333,14.41666667),
            new StarPosition("M100", "(GX)",12.38166667,15.81666667),
            new StarPosition("M101", "Pinwheel Nebula (GX)",14.05333333,54.35),
            new StarPosition("M102", "Spindle Galaxy (GX)",15.108194444,55.763333333),
            new StarPosition("M103", "(OC)",1.553333333,60.7),
            new StarPosition("M104", "Sombrero Galaxy (GX)",12.66666667,-11.61666667),
            new StarPosition("M105", "(GX)",10.79666667,12.58333333),
            new StarPosition("M106", "(GX)",12.31666667,47.3),
            new StarPosition("M107", "(GC)",16.54166667,-13.05),
            new StarPosition("M108", "(GX)",11.19333333,55.68333333),
            new StarPosition("M109", "(GX)",11.96,53.36666667),
            new StarPosition("M110", "(GX)",0.671666667,41.68333333)
        };
    }
}
