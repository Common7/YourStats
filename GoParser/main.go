// Copyright (c) 2026 Joshua Grossman
// Licensed under the MIT License. See LICENSE in the project root for license information.

package main

import (
	"encoding/json"
	"fmt"
	"os"

	demoinfocs "github.com/markus-wa/demoinfocs-golang/v5/pkg/demoinfocs"
	events "github.com/markus-wa/demoinfocs-golang/v5/pkg/demoinfocs/events"
	"github.com/markus-wa/demoinfocs-golang/v5/pkg/demoinfocs/msg"
)

type PlayerStats struct {
	Name          string `json:"Name"`
	SteamID       uint64 `json:"SteamID"`
	Team          int    `json:"Team"`
	Kills         int    `json:"Kills"`   // New: Standard Kills
	Deaths        int    `json:"Deaths"`  // New: Standard Deaths
	Assists       int    `json:"Assists"` // New: Standard Assists
	UtilityDamage int    `json:"UtilDamage"`
	FirstKills    int    `json:"FirstKills"`
}

type MatchStats struct {
	Map     string `json:"MapName"`
	CTScore int    `json:"CTScore"`
	TScore  int    `json:"tScore"`
}

func main() {
	if len(os.Args) < 2 {
		fmt.Println("Usage: go run main.go <path-to-demo>")
		return
	}

	matchData := MatchStats{}

	demoPath := os.Args[1]

	f, _ := os.Open(demoPath)
	defer f.Close()

	p := demoinfocs.NewParser(f)
	defer p.Close()

	statsMap := make(map[uint64]*PlayerStats)
	isFirstKillOfRound := true

	// General helper to get or create a player entry in our map
	getStats := func(id uint64, name string) *PlayerStats {
		if statsMap[id] == nil {
			statsMap[id] = &PlayerStats{Name: name, SteamID: id}
		}
		return statsMap[id]
	}

	// TRACK KILLS, DEATHS, AND FIRST KILLS
	p.RegisterEventHandler(func(e events.Kill) {
		// 1. Track standard Kill for the attacker
		if e.Killer != nil {
			killer := getStats(e.Killer.SteamID64, e.Killer.Name)
			killer.Kills++

			// 2. Track First Kill logic
			if isFirstKillOfRound {
				killer.FirstKills++
				isFirstKillOfRound = false
			}
		}

		// 3. Track standard Death for the victim
		if e.Victim != nil {
			victim := getStats(e.Victim.SteamID64, e.Victim.Name)
			victim.Deaths++
		}

		// 4. Track Assist
		if e.Assister != nil {
			assister := getStats(e.Assister.SteamID64, e.Assister.Name)
			assister.Assists++
		}
	})

	p.RegisterEventHandler(func(e events.RoundStart) {
		isFirstKillOfRound = true
	})

	// TRACK UTILITY DAMAGE
	p.RegisterEventHandler(func(e events.PlayerHurt) {
		// weapon.Class() 6 represents Grenades/Projectiles in demoinfocs
		if e.Attacker != nil && e.Weapon != nil && e.Weapon.Class() == 6 {
			id := e.Attacker.SteamID64
			stats := getStats(id, e.Attacker.Name)
			stats.UtilityDamage += e.HealthDamage
		}
	})

	p.RegisterNetMessageHandler(func(m *msg.CSVCMsg_ServerInfo) {
		fmt.Println("Map:", m.GetMapName())
		matchData.Map = m.GetMapName()
	})

	p.ParseToEnd()

	matchData.CTScore = p.GameState().TeamCounterTerrorists().Score()
	matchData.TScore = p.GameState().TeamTerrorists().Score()

	for _, p := range p.GameState().Participants().All() {
		if p.SteamID64 != 0 {
			stats := statsMap[p.SteamID64]
			stats.Team = int(p.Team)
			statsMap[p.SteamID64] = stats
		}
	}

	//out match data
	matchJson, _ := json.MarshalIndent(matchData, "", "  ")
	os.WriteFile("match.json", matchJson, 0644)

	// out stats
	output, _ := json.Marshal(statsMap)
	os.WriteFile("result.json", output, 0644)
}
