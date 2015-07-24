//
//  DbQueue.h
//  ToDoList
//
//  Created by Igi on 2/4/15.
//  Copyright (c) 2015 Igi. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "DbManager.h"

@interface DbQueue : NSObject

- (void) schedule:(NSDictionary *)command;
- (NSArray *) pop;
- (NSArray *) pop:(int)limit;
- (BOOL) isEmpty;
- (void) clear:(NSArray *)successfull andFailed:(NSArray *)failed;

@end
