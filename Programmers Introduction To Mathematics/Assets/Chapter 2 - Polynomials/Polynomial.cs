using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Polynomial : Equation
{
    public int Degree
    {
        get
        {
            return Terms[Terms.Count - 1].Degree;
        }
    }

    public List<PolynomialTerm> Terms
    {
        get;
        protected set;
    }


    public Polynomial()
    {
        Terms = new List<PolynomialTerm>();

        Terms.Add(new PolynomialTerm(0, 0));
    }

    public Polynomial(float[] coefficients, bool discardZeroCoefficients = true)
    {
        Terms = new List<PolynomialTerm>();

        for (int power = 0; power < coefficients.Length; power++)
        {
            //makes equations more readable, but sometimes terms that resolve to zero are desirable
            if (coefficients[power] == 0 && discardZeroCoefficients)
            {
                continue;
            }
            
            Terms.Add(new PolynomialTerm(power, coefficients[power]));
        }
    }

    public static Polynomial FromRoots(float[] roots)
    {
        Polynomial firstRoot = Polynomial.LinearWithZeroAt(roots[0]);

        Polynomial result = firstRoot;

        for(int i = 1; i < roots.Length; i++)
        {
            result *= Polynomial.LinearWithZeroAt(roots[i]);
        }

        return result;
    }

    public static Polynomial LinearWithZeroAt(float zeroCrossing)
    {
        return new Polynomial(
            new float[]
            {
                -zeroCrossing, 1
            });
    }

    public static void Interpolate(Polynomial from, Polynomial to, ref Polynomial display,  float t)
    {

        for(int term = 0; term < to.Terms.Count; term++)
        {
            display.Terms[term].Coefficient = Mathf.Lerp(
                    from.Terms[term].Coefficient,
                    to.Terms[term].Coefficient,
                    t);
        }
    }

    public static Polynomial Interpolate(Polynomial from, Polynomial to, float t)
    {
        Polynomial result = new Polynomial();

        for (int termNumber = 0; termNumber < to.Terms.Count; termNumber++)
        {
            float coefficient = Mathf.Lerp(
                    from.Terms[termNumber].Coefficient,
                    to.Terms[termNumber].Coefficient,
                    t);
            int degree = termNumber;

            result += new PolynomialTerm(degree, coefficient);
        }

        return result;
    }

    /*
    public static Polynomial ConstructFromPoints(Vector2[] points)
    {
        Polynomial polynomial = new Polynomial();

        foreach(Vector2 point in points)
        {
            
        }
    }
    */

    public static Polynomial operator +(Polynomial polynomialA, Polynomial polynomialB)
    {
        foreach(PolynomialTerm termB in polynomialB.Terms)
        {
            polynomialA = polynomialA + termB;
        }

        return polynomialA;
    }

    public static Polynomial operator +(Polynomial polynomialA, PolynomialTerm termB)
    {
        int terms = polynomialA.Terms.Count;
        for (int termNumber = 0; termNumber < terms; termNumber++)
        {
            PolynomialTerm currentTerm = polynomialA.Terms[termNumber];

            //add like coefficients
            if (currentTerm.Degree == termB.Degree)
            {
                polynomialA.Terms[termNumber].Coefficient += termB.Coefficient;
                return polynomialA;
            }

            //insert new term at correct position
            if (currentTerm.Degree > termB.Degree)
            {
                polynomialA.Terms.Insert(termNumber, termB);
                return polynomialA;
            }

        }

        //otherwise, term belongs at the end
        polynomialA.Terms.Add(termB);
        return polynomialA;
    }

    public static Polynomial operator *(Polynomial polynomialA, Polynomial polynomialB)
    {
        Polynomial product = new Polynomial();
        foreach(PolynomialTerm termB in polynomialB.Terms)
        {
            product += polynomialA * termB;
        }

        return product;
    }

    public static Polynomial operator *(Polynomial polynomialA, PolynomialTerm termB)
    {
        Polynomial product = new Polynomial();

        foreach(PolynomialTerm termA in polynomialA.Terms)
        {
            product += (termA * termB);
        }

        return product;
    }

    public static Polynomial operator *(Polynomial polynomial, float value)
    {
        Polynomial product = new Polynomial();
        foreach(PolynomialTerm term in polynomial.Terms)
        {
            product += term * value;
        }

        return product;
    }

    public static Polynomial operator /(Polynomial polynomial, float divisor)
    {
        Polynomial result = new Polynomial();
        foreach(PolynomialTerm term in polynomial.Terms)
        {
            result += (term / divisor);
        }

        return result;
    }

    public virtual float Evaluate(float x)
    {
        float sum = 0;
        foreach(PolynomialTerm term in Terms)
        {
            sum += term.Evaluate(x);
        }

        return sum;
    }

    public override string ToString()
    {
        string result = "y = ";

        for (int termNumber = 0; termNumber < Terms.Count; termNumber++)
        {
            if(termNumber != 0)
            {
                result += " + ";
            }

            result += Terms[termNumber].ToString();
        }

        return result;
    }
}

public class PolynomialTerm : Equation
{
    public int Degree;

    public float Coefficient;

    public PolynomialTerm(int _degree, float _coefficient)
    {
        Degree = _degree;
        Coefficient = _coefficient;
    }

    public float Evaluate(float x)
    {
        return Coefficient * (Mathf.Pow(x, Degree));
    }

    public static PolynomialTerm operator *(PolynomialTerm term, float value)
    {
        return new PolynomialTerm(term.Degree, term.Coefficient * value);
    }

    public static PolynomialTerm operator *(PolynomialTerm termA, PolynomialTerm termB)
    {
        return new PolynomialTerm(termA.Degree + termB.Degree, termA.Coefficient * termB.Coefficient);
    }

    public static PolynomialTerm operator /(PolynomialTerm term, float divisor)
    {
        return new PolynomialTerm(term.Degree, term.Coefficient / divisor);
    }

    public override string ToString()
    {
        if (Degree == 0)
        {
            return Coefficient.ToString();
        }

        return string.Format("{0}x^{1}", Coefficient, Degree);
    }
}

public interface Equation
{
    public float Evaluate(float x);
}

public class EquationWrapper : Equation {
    public Polynomial equation;

    public float Evaluate(float x)
    {
        return equation.Evaluate(x);
    }
}
