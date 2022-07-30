﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFireLaserBehavior
{
    void DisableFiring();
    void Fire(Transform target);
}