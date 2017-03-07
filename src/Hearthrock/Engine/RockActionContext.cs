﻿using Hearthrock.Contracts;
using Hearthrock.Serialization;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Hearthrock.Engine
{
    class RockActionContext
    {
        RockAction rockAction;
        int step;

        public RockActionContext(RockAction rockAction)
        {
            this.rockAction = rockAction;
            this.step = 0;
        }

        public string Interpretion(GameState gameState)
        {
            var sourceEnity = GetEntity(gameState, this.rockAction.Source);
            if (this.rockAction.Targets.Count == 0)
            {
                return "Play: " + sourceEnity.GetName();
            }
            else
            {
                var targetEnities = new List<Entity>();
                foreach (var rockId in this.rockAction.Targets)
                {
                    targetEnities.Add(GetEntity(gameState, rockId));
                }

                string ret = "Attack: " + sourceEnity.GetName() + " ";
                foreach (var targetEnity in targetEnities)
                {
                    ret += " > " + targetEnity.GetName();
                }

                return ret;
            }
        }

        public void Apply(GameState gameState, HearthrockEngine engine)
        {

            engine.Trace(RockJsonSerializer.Serialize(this.rockAction));
            engine.Trace(this.step.ToString());


            // Pick source card
            if (this.step == 0)
            {
                //RockInputManager.DisableInput();
                RockPegasusClient.ClickCard(GetCard(gameState, this.rockAction.Source));

                this.step = 1;
                return;
            }

            if (this.step == 1 && this.rockAction.Targets.Count == 0)
            {
                // InputManager.Get().DoNetworkResponse(GetCard(gameState, this.rockAction.Source).GetEntity(), true);
                RockPegasusClient.DropCard();
                //RockInputManager.EnableInput();

                this.step = 2;
                return;
            }

            // other scenarios
            if (this.rockAction.Targets.Count >= this.step)
            {
                RockPegasusClient.ClickCard(GetCard(gameState, this.rockAction.Targets[this.step - 1]));
                this.step++;
                return;
            }
            else
            {
                RockPegasusClient.DropCard();
                //RockInputManager.EnableInput();
                this.step++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsInvalid(GameState gameState)
        {
            if (null == GetCard(gameState, this.rockAction.Source))
            {
                return true;
            }

            foreach (var rockId in this.rockAction.Targets)
            {
                if (null == GetCard(gameState, rockId))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool IsDone()
        {
            return (this.step > this.rockAction.Targets.Count + 1);
        }

        public static Card GetCard(GameState gameState, int rockId)
        {
            return GameState.Get().GetEntity(rockId)?.GetCard();
        }

        public static Entity GetEntity(GameState gameState, int rockId)
        {
            return GameState.Get().GetEntity(rockId);
        }
    }
}