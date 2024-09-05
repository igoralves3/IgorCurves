using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class ShipMove : MonoBehaviour
{
    public struct Point
    {
        

        public float x {  get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public bool IsStar { get; set; }

        public Point(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.IsStar = false;
        }

        public Point(float x, float y, float z, bool isStar)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.IsStar = isStar;
        }

        public static Point operator -(Point a, Point b) => new Point(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Point operator *(float scalar, Point p) => new Point(scalar * p.x, scalar * p.y, scalar - p.z);
        public static Point operator +(Point a, Point b) => new Point(a.x + b.x, a.y + b.y, a.z + b.z);

    }

    public enum EasingType
    {
         NoEasing  ,
         EasyIn ,
         EasyOut 

    }

    public EasingType currentEasing;
    public float defaultSpeed = 0.01f;
    public float speed = 0.01f;
    public float minSpeed = 0.0f;
    public float maxSpeed = 1f;
    public float deltaSpeed = 0.01f;
    public float delta = 0.01f;
    List<GameObject> points;

    public GameObject interpoint;

    private int delayFrames = 0;
    private int curIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        
        speed = defaultSpeed;

        delayFrames = 0;

        curIndex = 0;

        
        points = new List<GameObject>();
        var estrelas = GameObject.FindGameObjectsWithTag("Star");

        var p0 = Instantiate(interpoint,transform.position, Quaternion.identity);

        points.Add(p0);

        

        Vector3 currentStarPosition =transform.position;

       
        Point pInicial = new Point(transform.position.x, transform.position.y, transform.position.z);
        //Point pSecond = new Point(currentStarPosition.x, currentStarPosition.y, currentStarPosition.z);
        Point ti = CalculaTangentePartida(pInicial);
        Point te;



        for (int i = 0; i < estrelas.Length; i++)
        {
            var proximaEstrela = estrelas[i].transform.position;
            GameObject proximoPontoEstrela = Instantiate(interpoint, new Vector3(proximaEstrela.x, proximaEstrela.y, proximaEstrela.z), Quaternion.identity);
            
           


            var pontoAtual = points[points.Count - 1];

            Point pi = new Point(pontoAtual.transform.position.x, pontoAtual.transform.position.y, pontoAtual.transform.position.z);
            Point pe = new Point(proximaEstrela.x, proximaEstrela.y, proximaEstrela.z);


            currentStarPosition = estrelas[i].transform.position;




            te = CalculaTangenteChegada(pi, pe, ti);
            ti = te;

            
            Point nextPoint;
            GameObject gnext;
            speed = 0;
            deltaSpeed = 0;
            switch (currentEasing) {

                case EasingType.NoEasing:
                while (speed < 1)
                {
                    nextPoint = CurveHermitePoint(pi, pe, ti, te, speed);
                    gnext = Instantiate(interpoint, new Vector3(nextPoint.x, nextPoint.y, nextPoint.z), Quaternion.identity);


                    points.Add(gnext);

                    
                    deltaSpeed+=delta;
                    speed=deltaSpeed;

                }
                points.Add(proximoPontoEstrela);
                break;

                case EasingType.EasyIn:

                    
                    while (speed < 1)
                    {
                        nextPoint = CurveHermitePoint(pi, pe, ti, te, speed);
                        gnext = Instantiate(interpoint, new Vector3(nextPoint.x, nextPoint.y, nextPoint.z), Quaternion.identity);


                        points.Add(gnext);

                        

                        deltaSpeed+=delta;
                        speed = deltaSpeed * deltaSpeed * deltaSpeed;


                        

                    }
                    
                    points.Add(proximoPontoEstrela);

                    break;

                case EasingType.EasyOut:
                    
                    while (speed < 1)
                    {
                        nextPoint = CurveHermitePoint(pi, pe, ti, te, speed);
                        gnext = Instantiate(interpoint, new Vector3(nextPoint.x, nextPoint.y, nextPoint.z), Quaternion.identity);


                        points.Add(gnext);

                        

                        deltaSpeed+=delta;
                        speed = 1 - Mathf.Pow(1-deltaSpeed,3);

                        

                    }
                    
                    points.Add(proximoPontoEstrela);

                    break;

                default: break;
        }
        }

        Vector3 direction = transform.position - points[curIndex].transform.position;


        // Calcular o ângulo de rotação
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

        // Aplicar a rotação
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        

        Debug.Log(transform.rotation.ToString());
    }


    

    Point Lerp(Point p1, Point p2, float t)
    {
        Vector3 v1 = new Vector3(p1.x,p1.y,p1.z);
        Vector3 v2 = new Vector3(p2.x, p2.y,p2.z);

        float newX = (1 - t) * v1.x + t * v2.x;
        float newY = (1 - t) * v1.y + t * v2.y;
        float newZ = (1 - t) * v1.z + t * v2.z;

       

        return new Point(newX,newY,newZ);
    }

    Point QuadraticLerp(Point p1, Point p2, Point p3, float t)
    {
        Point p1t = Lerp(p1,p2,t);

        Point p2t = Lerp(p2,p3,t);

        Point pMiddle = Lerp(p1t,p2t,t);

        return pMiddle;
    }

    Point CubicLerp(Point p1, Point p2, Point p3, Point p4, float t)
    {

        Point D = QuadraticLerp(p1,p2,p3,t);
        Point E = QuadraticLerp(p2, p3,p4, t);
        return Lerp(D,E,t);

    }

    Point CurveHermitePoint(Point p0, Point p1, Point t0, Point t1, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        Point h00 = (2 * t3 - 3 * t2 + 1) * p0;
        Point h01 = (t3 - 2 * t2 + t) * t0;
        Point h10 = (-2 * t3 + 3 * t2) * p1;
        Point h11 = (t3 - t2) * t1;

        return h00 + h01 + h10 + h11;
    }

    

    public static Point CalculaTangentePartida(Point t0)
    {
        return 3 * t0;
    }

    Point CalculaTangenteChegada(Point p0, Point p1, Point t0)
    {
        Point chegada = (p1 - p0) + t0;
        return chegada;
    }

    
    // Update is called once per frame
    void Update()
    {
        
        delayFrames++;
        if (delayFrames >= 10)
        {
            delayFrames = 0;
            curIndex++;

            if (curIndex < points.Count)
            {

                this.transform.position = new Vector3(points[curIndex].transform.position.x, points[curIndex].transform.position.y, points[curIndex].transform.position.z);


            }

            if (curIndex < points.Count - 1)
            {
                //Vector3 look = transform.InverseTransformPoint(points[curIndex + 1].transform.position.x, points[curIndex + 1].transform.position.y, points[curIndex + 1].transform.position.z);
                //float angle = Mathf.Atan2(look.x, look.y) * Mathf.Rad2Deg - 90;
                Vector3 direction = points[curIndex + 1].transform.position - transform.position;


                // Calcular o ângulo de rotação
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;

                // Aplicar a rotação
                transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            }


        }
        
    }
}
