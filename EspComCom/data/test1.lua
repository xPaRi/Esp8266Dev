-- Testy rychlosti

function Test1()
    gpio.mode(1, gpio.OUTPUT, gpio.PULLUP);

    print("Test 1 start")

    for i=1, 100 do
        gpio.write(1, gpio.LOW)
        gpio.write(1, gpio.HIGH)
    end

    print("end")
end

function Test2()
    gpio.mode(1, gpio.OUTPUT, gpio.PULLUP);

    print("Test 2 start")

    gpio.serout(1,1,{30,30,60,60,30,30})

    print("end")
end

function Test3()
    gpio.mode(1, gpio.OUTPUT, gpio.PULLUP);

    print("Test 3 start")

    gpio.serout(1,0,{20,20,1,2,3,4,5,6,7,8,20,20})

    print("end")
end

Test1()