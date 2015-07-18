using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trik;
using Trik.Collections;
using Trik.Devices;
using Trik.Sensors;
using Trik.Reactive;



namespace Лестница
{
    class Program
    {
        static void Main(string[] args)
        {
            var model = new Model();
            var kind = new ServoKind(stop : 0, zero : 1505000, min :  800000, max : 2210000, period : 20000000);
            //{ stop = 0; zero = 1550000; min =  800000; max = 2250000; period = 20000000 }
            model.ServosConfig[ServoPort.E1] = kind;
            
            
            

            //////////////////////Отключение по кнопке

            var buttons = model.Buttons;
            buttons.Start();

            var isEnterPressed = false;
            
            Action<ButtonEvent> enterFlagSetter = (ButtonEvent x) =>
            {
                
                if (x.Button == ButtonEventCode.Enter)
                {
                    isEnterPressed = true;
                }
                else
                {
                    isEnterPressed = false;
                }
            };

            ////Func<string, int> func1 = (x => 0);
            ////Func<string, int> func2 = (x => x.Length);
            ////Func<string, int> func3 = x => 
            ////    {

            ////       return x.Length;
            ////    };
            

            ////func2.Invoke("asdasd");
            ////func2("asdad");


            buttons.ToObservable().Subscribe(enterFlagSetter);


            //////////////////////////////////// Программа
            
            ////////// PD регулятор

            double k1 = 0.5;    //P
            double k2 = 0.03;      //D

            

            double angserv = -100;   // значение сервы
            int angservout = (int) angserv;     // для преобразования типов

            model.Accel.Start();
            model.Servos[ServoPort.E1].SetPower(angservout);
            Thread.Sleep(500);
            
            

            int time = 300;  // период цикла
            double angd;   // значение угла по акселерометру
            double ang = 90;   // необходимое значение угла трика
            double dev = 0;   //производная
            
            ang*=Math.PI/180;
            angserv*=90/100*Math.PI/180;

            //Console.WriteLine("Start");

            while (!isEnterPressed)
            {
                angd = Math.Atan2(model.Accel.Read().X, model.Accel.Read().Z);
                dev = (angd - dev) / time*1000;
                angserv += k1 * (ang - angd) - k2 * dev;
                dev = angd;

                if (Math.Abs(angserv) > 100)
                    angserv = Math.Sign(angserv) * 100;

                angservout = (int)(angserv * 180 / Math.PI * 100 / 90);

                model.Servos[ServoPort.E1].SetPower(angservout);

                //Console.Write("angd: "); Console.WriteLine(angd * 180 / Math.PI);
                //Console.Write("angserv: "); Console.WriteLine(angserv * 180 / Math.PI);
                
                Thread.Sleep(time);
            }

            model.Accel.Stop();
            Thread.Sleep(300);
            
        

        }
    }
}
