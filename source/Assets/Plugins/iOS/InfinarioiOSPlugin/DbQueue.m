//
//  DbQueue.m
//  ToDoList
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Igi. All rights reserved.
//

#import "DbQueue.h"

static int const MAX_RETRIES = 20;
static int const POP_LIMIT = 50;

@interface DbQueue ()

@property DbManager *dbManager;
@property NSObject *lockAccess;

@end

@implementation DbQueue

- (instancetype) init {
    self = [super init];
    
    self.lockAccess = [[NSObject alloc] init];
    self.dbManager = [[DbManager alloc] initWithDatabaseFilename: DB_FILE];
    [self createDbIfNecessary];
    
    return self;
}

- (void) schedule:(NSDictionary *)command {
    @synchronized(self.lockAccess){
        NSData *data = [NSJSONSerialization dataWithJSONObject:command options:0 error:nil];
        NSString *str = [[NSString alloc] initWithData:data encoding:NSUTF8StringEncoding];
        [self.dbManager executeQuery:[NSString stringWithFormat: @"INSERT INTO commands (command) VALUES ('%@');", str]];
    }
}

- (NSArray *) pop:(int) limit {
    @synchronized(self.lockAccess){
        NSMutableArray *requests = [[NSMutableArray alloc] init];
        NSMutableDictionary *request;
        NSMutableDictionary *commandData;
        NSData *data;
    
        NSArray *commands = [self.dbManager loadDataFromDb:[NSString stringWithFormat:@"SELECT * FROM commands LIMIT %d;", limit]];

        for (NSMutableArray *command in commands) {
            data = [command[1] dataUsingEncoding:NSUTF8StringEncoding];
            commandData =[NSJSONSerialization JSONObjectWithData:data options:NSJSONReadingMutableContainers error:nil];
        
            request = [[NSMutableDictionary alloc] init];
        
            request[@"id"] = command[0];
            request[@"command"] = commandData;
        
            [requests addObject:request];
        }
    
        return requests;
    }
}

- (NSArray *) pop {
        return [self pop:POP_LIMIT];
}

- (BOOL) isEmpty {
    @synchronized(self.lockAccess){
        NSArray *result = [self.dbManager loadDataFromDb:@"SELECT COUNT(*) FROM commands;"];

        return [result[0][0] isEqualToString:@"0"];
    }
}

- (void) clear:(NSArray *)successfull andFailed:(NSArray *)failed {
    @synchronized(self.lockAccess){
        [self.dbManager executeQuery:[NSString stringWithFormat:@"DELETE FROM commands WHERE id IN (%@)", [successfull componentsJoinedByString:@","]]];
        [self.dbManager executeQuery:[NSString stringWithFormat:@"UPDATE commands SET retries = retries + 1 WHERE id IN (%@);", [failed componentsJoinedByString:@","]]];
        [self.dbManager executeQuery:[NSString stringWithFormat:@"DELETE FROM commands WHERE retries > %d", MAX_RETRIES]];
    }
}

- (void) createDbIfNecessary {
    @synchronized(self.lockAccess){
        [self.dbManager executeQuery:@"CREATE TABLE IF NOT EXISTS commands ("
                                    " id INTEGER PRIMARY KEY AUTOINCREMENT,"
                                    " command TEXT NOT NULL,"
                                    " retries INTEGER NOT NULL DEFAULT 0);"];
    }
}

@end
